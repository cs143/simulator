using IP = System.Int32;
using System;

namespace simulator {

public class Host:DumbNode {
    // STATIC
    private static IP next_ip = 100;

    // SHARED
    public readonly EventQueueProcessor eqp;
    public readonly IP ip = Host.next_ip++;
    public readonly string name;
    public Link link { get; set; }
    private int expected_seq_num = 0; // seq number for the next packet
    public FlowReceive flow_rec_stat = new FlowReceive();
    public HostStatus hStat = new HostStatus();

    // SENDER SPECIFIC
    public double window_size { get; protected set; }
    private Int64 bits_to_send = 0;
    private IP dest_ip;
    private int next_seq_num = 0;
    private int ack_num = 0;
    private double timeout = 1; // seconds
    private TCPStrategy tcp_strat;

    // PUBLIC METHODS
    /* Initializer */
    public Host(EventQueueProcessor eqp, string name) {
        this.eqp = eqp;
        this.name = name;
        this.hStat.host_name = name;
        this.hStat.flows = new FlowStatus[1];
        this.hStat.flows[0] = new FlowStatus();
    }
    Random r = new Random();

    /* Main receive event */
    public Event ReceivePacket(Packet packet) {
        return () => {
            if (packet.type == PacketType.ACK) {
                ProcessACKPacket(packet);
            } else {
                if (r.Next(0, 20) != 0) {
                    ProcessDataPacket(packet);
                } else {
                    Console.WriteLine(name+": Ignoring " + packet + this);
                }
            }
        };
    }

    // TODO strategy
    public Event SetupSend(IP ip, Int64 bits_to_send) {
        return () => {
            this.bits_to_send = bits_to_send;
            this.dest_ip = ip;
            this.tcp_strat = new TCPReno();
            UpdateTCPState();
            eqp.Add(eqp.current_time, SendPacket());
        };
    }

    public override string ToString() {
        string tmpl = "<Host ip={0} name={1} window_size={2:0.00} seq_num={3}";
        tmpl += " ack_num={4} bits_to_send={5}>";
        return string.Format(tmpl, ip, name, window_size, next_seq_num, ack_num, bits_to_send);
    }

    // PRIVATE METHODS THAT DO NOT USE TCP STRATEGY
    private Event SendPacket() {
        return () => {
            if (HasPacketsToSend()) _SendPacket();
        };
    }

    private bool HasPacketsToSend() {
        if (this.next_seq_num * Packet.DEFAULT_PAYLOAD_SIZE < this.bits_to_send &&
            this.next_seq_num - this.ack_num < this.window_size) {
            return true;
        }
        return false;
    }

    private void _SendPacket() {
        var packet = new Packet{
            payload_size=Packet.DEFAULT_PAYLOAD_SIZE,
            src=this.ip,
            dest=this.dest_ip,
            type=PacketType.DATA,
            seq_num=this.next_seq_num,
            timestamp = eqp.current_time
        };
        // TODO re-implement
        Console.WriteLine(name+":"+eqp.current_time+": Sending " + packet);
        double completion_time = eqp.current_time + packet.size/link.rate;
        eqp.Add(completion_time, link.ReceivePacket(packet));
        this.next_seq_num += 1;
        // if HasPacketsToSend() == false, it will be idempotent
        eqp.Add(completion_time, SendPacket());
        eqp.Add(completion_time + this.timeout, CheckTimeout(packet));
        hStat.flows[0].time = eqp.current_time;
        hStat.flows[0].window_size = window_size;
        Logger.LogHostStatus(hStat);
    }

    private void ProcessDataPacket(Packet packet) {
        if (packet.seq_num == expected_seq_num) {
            expected_seq_num++;
        }
        var ack_p = new Packet{payload_size=Packet.DEFAULT_ACK_SIZE,
                                src=this.ip,
                                dest=link.dest.ip,
                                type=PacketType.ACK,
                                seq_num=expected_seq_num};
        eqp.Add(eqp.current_time + ack_p.size / link.rate, link.ReceivePacket(ack_p));
    }

    // PRIVATE METHODS THAT USE TCP STRATEGY
    private void ProcessACKPacket(Packet packet) {
        this.tcp_strat.ProcessAck(packet, eqp.current_time);
        UpdateTCPState();
        eqp.Add(eqp.current_time, SendPacket());
    }

    private Event CheckTimeout(Packet packet) {
        return () => {
            if (this.ack_num <= packet.seq_num) { // timed out
                this.tcp_strat.ProcessTimeout(packet, eqp.current_time);
                UpdateTCPState();
            }
            eqp.Add(eqp.current_time, SendPacket());
        };
    }

    // updates host parameters
    private void UpdateTCPState() {
        this.window_size = this.tcp_strat.WindowSize();
        this.timeout = this.tcp_strat.Timeout();
        this.ack_num = this.tcp_strat.BiggestAck();
        if (this.tcp_strat.ResetSeq()) {
            this.next_seq_num = this.ack_num;
        }
    }
}

}
