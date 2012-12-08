using IP = System.Int32;
using System;

namespace simulator {

public class Host:DumbNode {
    // STATIC

    // SHARED
    public readonly EventQueueProcessor eqp;
    private int expected_seq_num = 0; // seq number for the next packet
    public FlowReceive flow_rec_stat = new FlowReceive();
    public HostStatus hStat = new HostStatus();

    // SENDER SPECIFIC
    public double window_size { get; protected set; }
    private Int64 bits_to_send = 0;
    private IP dest_ip;
    private int next_seq_num = 0;
    private int ack_num = 0;
    // in seconds. we assume that the first packet will never time out.
    private double timeout = 10000000;
    // used in `CheckTimeout` we use this attr to 
    // ignore the timers set before window size reset
    private int window_resets = 0;

    private TCPStrategy tcp_strat;
    private bool is_busy = false;

    // PUBLIC METHODS
    /* Initializer */
    public Host(EventQueueProcessor eqp, string name) {
        this.eqp = eqp;
        this.name = name;
        this.hStat.host_name = name;
        this.hStat.flows = new FlowStatus[1];
        this.hStat.flows[0] = new FlowStatus();
    }

    /* Main receive event */
    public override Event ReceivePacket(Packet packet) {
        return () => {
            if (packet.type == PacketType.ACK) {
                ProcessACKPacket(packet);
            } else {
                //Random r = new Random();
                //if (r.Next(0, 20) != 0) {
                    ProcessDataPacket(packet);
                //} else {
                //    Console.WriteLine(name+": Ignoring " + packet + this);
                //}
            }
        };
    }

    // TODO strategy
    public Event SetupSend(IP ip, Int64 bits_to_send, string algo) {
        return () => {
            this.bits_to_send = bits_to_send;
            this.dest_ip = ip;
            if (algo == "reno") {
                this.tcp_strat = new TCPReno();
            } else if (algo == "fasttcp") {
                this.tcp_strat = new TCPFast();
                eqp.Add(eqp.current_time, UpdateWindowSize());
            }
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
            if (HasPacketsToSend() && !is_busy) _SendPacket();
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
        //Console.WriteLine(name+":"+eqp.current_time+": Sending " + packet);
        double completion_time = eqp.current_time + packet.size / link.rate;
        eqp.Add(eqp.current_time, link.ReceivePacket(packet));
        is_busy = true;
        this.next_seq_num += 1;
        // if HasPacketsToSend() == false, it will be idempotent
        eqp.Add(completion_time, CompleteSend());
        eqp.Add(completion_time + this.timeout, CheckTimeout(packet, window_resets));
        hStat.flows[0].time = eqp.current_time;
        hStat.flows[0].window_size = window_size;
        Logger.LogHostStatus(hStat);
    }

    private Event CompleteSend() {
        return () => {
            is_busy = false;
            eqp.Add(eqp.current_time, SendPacket());
        };
    }

    private Event UpdateWindowSize() {
        return () => {
            tcp_strat.UpdateWindowSize();
            this.window_size = this.tcp_strat.WindowSize();
            if (this.next_seq_num * Packet.DEFAULT_PAYLOAD_SIZE < this.bits_to_send) {
                eqp.Add(eqp.current_time + 0.020, UpdateWindowSize());
            }
        };
    }

    private void ProcessDataPacket(Packet packet) {
        //Console.WriteLine("GOT " + packet + expected_seq_num);
        if (packet.seq_num == expected_seq_num) {
            expected_seq_num++;
            // ack should be issued for every packet, but for now
            var ack_p = new Packet{payload_size=Packet.DEFAULT_ACK_SIZE,
                                    src=this.ip,
                                    dest=link.dest.ip,
                                    type=PacketType.ACK,
                                    seq_num=expected_seq_num,
                                    timestamp=packet.timestamp};
            // Console.WriteLine(name+":"+eqp.current_time+": Sending " + ack_p);
            // Console.WriteLine("SENDING " + ack_p);
            eqp.Add(eqp.current_time, link.ReceivePacket(ack_p));
        }
    }

    // PRIVATE METHODS THAT USE TCP STRATEGY
    private void ProcessACKPacket(Packet packet) {
        //Console.WriteLine(eqp.current_time + ": Received ack" + packet);
        if (packet.seq_num >= ack_num && packet.seq_num <= next_seq_num) {
            this.tcp_strat.ProcessAck(packet, eqp.current_time);
            UpdateTCPState();
            eqp.Add(eqp.current_time, SendPacket());
        }
        else {
            Console.WriteLine(eqp.current_time + ": Dropping ack" + packet);
        }
    }

    private Event CheckTimeout(Packet packet, int resets) {
        return () => {
            if (packet.seq_num >= ack_num &&
                packet.seq_num < next_seq_num &&
                window_resets == resets) {
                // timed out
                Console.WriteLine(eqp.current_time + "TIMED OUT " + packet);
                window_resets++;
                this.tcp_strat.ProcessTimeout(packet, eqp.current_time);
                UpdateTCPState();
                eqp.Add(eqp.current_time, SendPacket());
            }
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
        Console.WriteLine(eqp.current_time + "\t" + window_size + "\t" + this.tcp_strat);
    }

    private bool InsideWindow(Packet packet) {
        if (packet.seq_num >= ack_num && packet.seq_num < next_seq_num) {
            return true;
        }
        //Console.WriteLine("NOT INSIDE");
        return false;
    }
}

}
