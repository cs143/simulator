namespace simulator {

interface TCPStrategy {
    void ProcessAck(Packet packet);
    void ProcessTimeout(Packet packet);
    double WindowSize();
    double RTT();
    int BiggestAck();
    bool ResetSeq();
}

public class TCPReno : TCPStrategy {
    private double window_size = 1;
    private double rtt = 1000;
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
    public void ProcessAck(Packet pkt) {
        if (pkt.seq_num > this.biggest_ack) {
            this.biggest_ack = pkt.seq_num;
            this.dup_cnt = 0;
            if (slow_start) {
                window_size++;
                if (window_size > slow_start_thresh) {
                    slow_start = false;
                    System.Console.WriteLine("SS to CA :" + this);
                }
            } else { // Congestion Avoidance
                window_size += 1/window_size;
            }
            reset_seq = false;
        }
        else if (pkt.seq_num == this.biggest_ack) {
            this.dup_cnt++;
            if (dup_cnt == 3) {
                window_size = System.Math.Max(2.0, window_size/2.0);
                slow_start = false;
                reset_seq = true;
                System.Console.WriteLine("CA:" + this);
            }
            else {
                reset_seq = false;
            }
        }
    }

    /**
      set window_size to 1, and go into slow state
    */
    public void ProcessTimeout(Packet pkt) {
        slow_start_thresh = System.Math.Max(2.0, window_size / 2.0);
        window_size = 1.0;
        reset_seq = true;
        System.Console.WriteLine("SS:" + this);
    }

    public double WindowSize() { return window_size; } 
    public double RTT() { return rtt; } 
    public int BiggestAck() { return biggest_ack; }
    public bool ResetSeq() { return reset_seq; }

    public override string ToString() {
        string tmpl = "<TCPReno window_size={0:0.00} rtt={1} ack={2}";
        tmpl += " dup_cnt={3} slow_start={4} slow_start_thresh={5:0.00}>";
        return string.Format(tmpl, window_size, rtt, biggest_ack, dup_cnt,
                             slow_start, slow_start_thresh);
    }
}

}
