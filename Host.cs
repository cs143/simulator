using System;

using IP = System.String;

namespace simulator
{

public class Host : Node
{
    #region SHARED
    public Link link { get; set; }
    private int expected_seq_num = 0; // seq number for the next packet
    public FlowReceive flow_rec_stat = new FlowReceive();
    public HostStatus hStat = new HostStatus();
    #endregion

    #region SENDER SPECIFIC
    public double window_size { get; protected set; }
    private Int64 bits_to_send = 0;
    private IP dest_ip;
    private int next_seq_num = 0;
    private int ack_num = 0;
    private double timeout = 1; // seconds
    private TCPStrategy tcp_strat;
    #endregion

    #region PUBLIC METHODS
    /* Initializer */
    public Host(EventQueueProcessor eqp, IP ip) : base(eqp, ip) {
        this.hStat.host_name = ip;
        this.hStat.flows = new FlowStatus[1];
        this.hStat.flows[0] = new FlowStatus();
    }
    Random r = new Random();

    /* Main receive event */
    public override Event ReceivePacket(Packet packet) {
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
        return string.Format("<Host ip={0} window_size={2:0.00} seq_num={3} ack_num={4} bits_to_send={5}>",
            ip, null, window_size, next_seq_num, ack_num, bits_to_send);
    }
    #endregion
    
    #region PRIVATE METHODS THAT DO NOT USE TCP STRATEGY
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

    // FIXME
    private void _SendPacket() {
        var packet = new Packet{
            payload_size=Packet.DEFAULT_PAYLOAD_SIZE,
            src=this.ip,
            dest=this.dest_ip,
            type=PacketType.DATA,
            seq_num=this.next_seq_num,
            timestamp = eqp.current_time
        };
        System.Console.WriteLine(ip + " sending " + packet + " at " + eqp.current_time);
        link.EnqueuePacket(packet);
        this.next_seq_num += 1;
        // if HasPacketsToSend() == false, it will be idempotent
        double completion_time = Double.NaN; // FIXME
        eqp.Add(completion_time, SendPacket());
        eqp.Add(completion_time + this.timeout, CheckTimeout(packet));
        hStat.flows[0].time = eqp.current_time;
        hStat.flows[0].window_size = window_size;
        Logger.LogHostStatus(hStat);
    }

    private void ProcessDataPacket(Packet packet) {
        System.Console.WriteLine(ip + " received " + packet + " at " + eqp.current_time);
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
    #endregion

    #region PRIVATE METHODS THAT USE TCP STRATEGY
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
    #endregion
}

}
