using System;

using IP = System.String;

namespace simulator
{

public class Host : Node
{
    #region SHARED
    public override void RegisterLink(Link link) {
        if(this.link != null)
            throw new InvalidOperationException("This Host already has an outbound link");
        this.link = link;
    }
    private Link link { get; set; }
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
    // in seconds. we assume that the first packet will never time out.
    private double timeout = 10000000;
    // used in `CheckTimeout` we use this attr to 
    // ignore the timers set before window size reset
    private int window_resets = 0;

    private TCPStrategy tcp_strat;
    private bool is_busy = false;
    #endregion

    #region PUBLIC METHODS
    /* Initializer */
    public Host(EventQueueProcessor eqp, IP ip) : base(eqp, ip) {
        this.hStat.host_name = ip;
        this.hStat.flows = new FlowStatus[1];
        this.hStat.flows[0] = new FlowStatus();
        this.hStat.flows[0].flow_name = "";
        this.flow_rec_stat.flow_name = "";
    }

    /* Main receive event */
    Random r = new Random();
    int randomized = 0;
    public override Event ReceivePacket(Packet packet) {
        return () => {
            switch(packet.type) {
                case PacketType.ACK:
                    ProcessACKPacket(packet);
                break;
                case PacketType.DATA:
                    ProcessDataPacket(packet);
                break;
                default:
                    // ignore routing packets
                break;
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
        return string.Format("<Host ip={0} window_size={2:0.00} seq_num={3} ack_num={4} bits_to_send={5}>",
            ip, null, window_size, next_seq_num, ack_num, bits_to_send);
    }
    #endregion

    #region PRIVATE METHODS THAT DO NOT USE TCP STRATEGY
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
        //Simulator.Message(name+":"+eqp.current_time+": Sending " + packet);
        double completion_time = eqp.current_time + packet.size / link.rate;
        eqp.Add(eqp.current_time, link.EnqueuePacket(packet));
        is_busy = true;
        this.next_seq_num += 1;
        // if HasPacketsToSend() == false, it will be idempotent
        eqp.Add(completion_time, CompleteSend());
        eqp.Add(completion_time + this.timeout, CheckTimeout(packet, window_resets));
        hStat.flows[0].time = eqp.current_time;
        hStat.flows[0].window_size = window_size;
        //Logger.LogHostStatus(hStat);
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
        //Simulator.Message("GOT " + packet + expected_seq_num);
        if (packet.seq_num == expected_seq_num) {
            expected_seq_num++;
        }
        var ack_p = new Packet{payload_size=Packet.DEFAULT_ACK_SIZE,
                                src=this.ip,
                                dest=packet.src,
                                type=PacketType.ACK,
                                seq_num=expected_seq_num,
                                timestamp=packet.timestamp};
        // Simulator.Message(name+":"+eqp.current_time+": Sending " + ack_p);
        // Simulator.Message("SENDING " + ack_p);
        eqp.Add(eqp.current_time, link.EnqueuePacket(ack_p));
        this.flow_rec_stat.received_packets++;
    }
    #endregion

    #region PRIVATE METHODS THAT USE TCP STRATEGY
    private void ProcessACKPacket(Packet packet) {
        //Simulator.Message(eqp.current_time + ": Received ack" + packet);
        if (packet.seq_num >= ack_num && packet.seq_num <= next_seq_num) {
            this.tcp_strat.ProcessAck(packet, eqp.current_time);
            UpdateTCPState();
            if (this.tcp_strat.ResetSeq()) {
                window_resets++;
            }
        }
        else {
            Simulator.Message(eqp.current_time + ": Dropping ack" + packet);
        }
        eqp.Add(eqp.current_time, SendPacket());
    }

    private Event CheckTimeout(Packet packet, int resets) {
        return () => {
            if (packet.seq_num >= ack_num &&
                packet.seq_num < next_seq_num &&
                window_resets == resets) {
                // timed out
                Simulator.Message(eqp.current_time + "TIMED OUT " + packet);
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
        //Simulator.Message(ip + ":" + eqp.current_time + "\t" + window_size + "\t" + this.tcp_strat);
    }
    #endregion
    
    public override Event RecalculateLinkState(int seq_num) {
        return () => {
            this.link.CalculateCost();
            eqp.Add(eqp.current_time, this.link.EnqueuePacket(
                Packet.CreateLinkStateAdvertisement(seq_num, src: this, link: this.link, current_time: eqp.current_time)
            ));
            Simulator.Message("Host {0} sent link-state packet to {2} on link {1}", this.ip, this.link.name, this.link.dest.ip);
        };
    }
}

}
