using Time = System.Double;
using System;

namespace simulator {

interface TCPStrategy {
    void ProcessAck(Packet packet, Time current_time);
    void ProcessTimeout(Packet packet, Time current_time);
    double WindowSize();
    double Timeout();
    int BiggestAck();
    bool ResetSeq();
    void UpdateWindowSize();
}

public class TCPReno : TCPStrategy {
    static double ROUNDTRIP_AVG_RATE = 0.25; // between 0 and 1
    private double window_size = 1;
    private double rt_avg = 10000000000;
    private double rt_dev = 10000000000;
    private int biggest_ack = 0;
    private int dup_cnt = 0;
    private bool slow_start = true;
    private double slow_start_thresh = 100000000;
    private bool reset_seq = false;

    /**
    if pkt.ack_num > prev_acknowledged
        set prev_ack = pkt.ack_num, DA = 0
        if slow start
            window_size++
            if window_size > slow_start_thresh
                go into CA mode
        if CA
            window_size += 1/window_size
    if pkt.ack_num = prev_acknowledged
        dupl_acks ++
        if dupl_acks >= 3
            halve window size
            go into CA mode (meaning, on the next successful ack, window_size += 1/window_size)
    if pkt.ack_num < prev_ack
        we can ignore this, since eventually things will time out
    */

    public void UpdateWindowSize() { } // idempotent

    public void ProcessAck(Packet pkt, Time current_time) {
        if (pkt.seq_num == 1) { // first packet
            AdjustTimeout(current_time - pkt.timestamp, true);
        }
        else {
            AdjustTimeout(current_time - pkt.timestamp, false);
        }
        if (pkt.seq_num > this.biggest_ack) {
            this.biggest_ack = pkt.seq_num;
            if (dup_cnt > 2) {
                this.dup_cnt = 0;
                window_size = slow_start_thresh;   
                // finished fast recovery
                // time for congestion avoidance
                slow_start = false;
                reset_seq = true;
                Simulator.Message(this + ": TCP mode = CA");
            }
            else {
                this.dup_cnt = 0;
                if (slow_start) {
                    window_size++;
                    if (window_size > slow_start_thresh) {
                        slow_start = false;
                        Simulator.Message(this + ": Switching from SS to CA");
                    }
                } else { // Congestion Avoidance
                    window_size += 1/window_size;
                }
                reset_seq = false;
            }
        }
        else if (pkt.seq_num == this.biggest_ack) {
            this.dup_cnt++;
            if (dup_cnt == 2) {
                slow_start_thresh = System.Math.Max(2.0, window_size / 2.0);
                window_size = 1; // effectively resends a single packet
                slow_start = false;
                reset_seq = true;
                Simulator.Message(this + ": FAST TRANSMIT");
            }
            else {
                //window_size++;
                reset_seq = false;
            }
        }
    }


    /**
      set window_size to 1, and go into slow state
    */
    public void ProcessTimeout(Packet pkt, Time current_time) {
        if (window_size != 1.0) {
            slow_start_thresh = System.Math.Max(2.0, window_size / 2.0);
            window_size = 1.0;
        } else {
            // go into slow start
            dup_cnt = 0;
        }
        reset_seq = true;
        slow_start = true;
        Simulator.Message("SS:" + this);
    }

    private void AdjustTimeout(Time rt, bool reset) {
        // Textbook Page 92
        if (reset) {
            rt_avg = rt;
            rt_dev = rt;
        }
        else {
            double b = ROUNDTRIP_AVG_RATE;
            rt_avg = (1-b)*rt_avg + b*rt;
            rt_dev = (1-b)*rt_dev + b*System.Math.Abs(rt - rt_avg);
        }
    }

    public double WindowSize() { return window_size; } 
    public double Timeout() { return 2*rt_avg + 4*rt_dev;}
    public int BiggestAck() { return biggest_ack; }
    public bool ResetSeq() { return reset_seq; }

    public override string ToString() {
        string tmpl = "<TCPReno window_size={0:0.00} timeout={1} ack={2}";
        tmpl += " dup_cnt={3} slow_start={4} slow_start_thresh={5:0.00} rt_avg={6} rt_dev={7}>";
        return string.Format(tmpl, window_size, Timeout(), biggest_ack, dup_cnt,
                             slow_start, slow_start_thresh, rt_avg, rt_dev);
    }
}

public class TCPFast : TCPStrategy {
    static double ROUNDTRIP_AVG_RATE = 0.25; // between 0 and 1
    private double window_size = 1;
    private double rt_avg = 10000000;
    private double rt_dev = 10000000;
    private int biggest_ack = 0;
    private int dup_cnt = 0;
    private bool reset_seq = false;
    private double base_rtt = 1000;

    public void ProcessAck(Packet pkt, Time current_time) {
        if (pkt.seq_num == 1) { // first packet
            AdjustTimeout(current_time - pkt.timestamp, true);
        }
        else {
            AdjustTimeout(current_time - pkt.timestamp, false);
        }
        if (pkt.seq_num > this.biggest_ack) { 
            this.biggest_ack = pkt.seq_num;
            this.dup_cnt = 0;
            reset_seq = false;
        }
        else if (pkt.seq_num == this.biggest_ack) {
            this.dup_cnt++;
            if (dup_cnt == 3) { reset_seq = true; }
            else { reset_seq = false; }
        }
    }

    public void ProcessTimeout(Packet pkt, Time current_time) {
        reset_seq = true;
    }

    // call this every 20 ms
    public void UpdateWindowSize() {
        window_size = window_size * base_rtt / rt_avg + 3.0;
    }

    private void AdjustTimeout(Time rt, bool reset) {
        // Textbook Page 92
        base_rtt = System.Math.Min(rt, base_rtt);
        if (reset) {
            rt_avg = rt;
            rt_dev = rt;
        }
        else {
            double b = ROUNDTRIP_AVG_RATE;
            rt_avg = (1-b)*rt_avg + b*rt;
            rt_dev = (1-b)*rt_dev + b*System.Math.Abs(rt - rt_avg);
        }
    }

    public double WindowSize() { return window_size; } 
    public double Timeout() { return 1.2*rt_avg + 4*rt_dev;}
    public int BiggestAck() { return biggest_ack; }
    public bool ResetSeq() { return reset_seq; }

    public override string ToString() {
        string tmpl = "<TCPFast window_size={0:0.00} timeout={1} ack={2}";
        tmpl += " dup_cnt={3} ";
        return string.Format(tmpl, window_size, Timeout(), biggest_ack, dup_cnt);
    }
}

}
