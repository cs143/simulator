namespace simulator {

interface TCPStrategy {
    void ProcessAck(Packet packet);
    void ProcessTimeout(Packet packet);
    double WindowSize();
    double RTT();
    int BiggestAck();
}

public class TCPReno : TCPStrategy {
    private double window_size = 1;
    private double rtt = 1000;
    private int biggest_ack = 0;
    private int dup_cnt = 0;
    private bool slow_start = true;
    private double slow_start_thresh = 2;

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
        //System.Console.WriteLine("received " + pkt);
        if (pkt.seq_num > this.biggest_ack) {
            //System.Console.WriteLine("normal");
            this.biggest_ack = pkt.seq_num;
            this.dup_cnt = 0;
            if (slow_start) {
                window_size++;
                if (window_size > slow_start_thresh) slow_start = false;
            } else { // Congestion Avoidance
                window_size += 1/window_size;
            }
        }
        else if (pkt.seq_num == this.biggest_ack) {
            //System.Console.WriteLine("dup");
            this.dup_cnt++;
            if (dup_cnt == 3) {
                window_size /= 2;
                slow_start = false;
            }
        }
    }

    /**
      set window_size to 1, and go into slow state
    */
    public void ProcessTimeout(Packet pkt) {
        System.Console.WriteLine("Dropped " + pkt);
        slow_start_thresh = window_size / 2.0;
        window_size = 1;
        biggest_ack = pkt.seq_num;
    }

    public double WindowSize() { return window_size; } 
    public double RTT() { return rtt; } 
    public int BiggestAck() { return biggest_ack; }

    public override string ToString() {
        string tmpl = "<TCPReno window_size={0:0.00} rtt={1} ack={2}";
        tmpl += " dup_cnt={3} slow_start={4} slow_start_thresh={5:0.00}>";
        return string.Format(tmpl, window_size, rtt, biggest_ack, dup_cnt,
                             slow_start, slow_start_thresh);
    }
}

}
