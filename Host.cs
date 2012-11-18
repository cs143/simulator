using IP = System.Int32;
using System;

namespace simulator {

public class Host {
    private static IP next_ip = 0; // HAX

    public readonly EventQueueProcessor eqp;
    public readonly IP ip = Host.next_ip++;
    public readonly string name;
    public Link link { get; set; }
    public FlowReceive flow_rec_stat = new FlowReceive();
    public HostStatus hStat = new HostStatus();

    public Host(EventQueueProcessor eqp, string name) {
        this.eqp = eqp;
        this.name = name;
        this.hStat.host_name = name;
        this.hStat.flows = new FlowStatus[1];
        this.hStat.flows[0] = new FlowStatus();
    }

    public virtual Event SendPacket() {
        // TODO what to do about seq_num?

        return () => {
            System.Console.WriteLine("shouldn't reach here gah. 2");
        };
    }

    public Event ReceivePacket(Packet packet) {
        return () => {
            if (packet.type == PacketType.ACK) {
                // TODO process ACK packet
                ProcessACKPacket(packet);
            } else {
                ProcessDataPacket(packet);
            }
        };
    }

    public Packet CreateAckPacket(IP dest) {
        // TODO account for dropped packets
        return new Packet{payload_size=0, src=this.ip,
                          dest=dest, type=PacketType.ACK};
    }

    public override string ToString() {
        return string.Format("<Host ip={0} name={1}>", ip, name);
    }
    
    public virtual void ProcessACKPacket(Packet packet) {
        System.Console.WriteLine("shouldn't reach here gah.");
    }

    private int next_num = 0;

    private void ProcessDataPacket(Packet packet) {
        if (packet.seq_num == next_num) {
            next_num++;
        }
        flow_rec_stat.time = eqp.current_time;
        flow_rec_stat.received_packets++;
        Logger.LogFlowRecStatus(flow_rec_stat);
        var ack_p = new Packet{payload_size=Packet.DEFAULT_ACK_SIZE,
                                dest=link.dest.ip,
                                type=PacketType.ACK,
                                seq_num=next_num};
        eqp.Add(eqp.current_time + ack_p.size / link.rate, link.ReceivePacket(ack_p));
    }

    // TODO will remove this
    public virtual Event SetupSend(IP ip, Int64 bits_to_send) {
        return () => {
        };
    }
}

public class HostAIMDSender : Host {
    private Int64 bits_to_send = 0;
    private IP dest_ip;
    public double window_size { get; protected set; }
    private int seq_num = 0;
    private int ack_num = 0;
    private double roundtrip_est = 1; // seconds

    public HostAIMDSender(EventQueueProcessor eqp, string name) : base(eqp, name) {
    }

    public override Event SetupSend(IP ip, Int64 bits_to_send) {
        return () => {
            this.bits_to_send = bits_to_send;
            this.dest_ip = ip;
            this.window_size = 1;
            eqp.Add(eqp.current_time, SendPacket());
        };
    }

    public override void ProcessACKPacket(Packet packet) {
        if (this.ack_num < packet.seq_num) {
            this.ack_num = packet.seq_num;
            this.window_size += 1 / this.window_size;
            // TODO adjust rtt estimate
            eqp.Add(eqp.current_time, SendPacket());
        }
    }

    public override Event SendPacket() {
        return () => {
            if (HasPacketsToSend()) _SendPacket();
        };
    }

    public bool HasPacketsToSend() {
        if (this.seq_num * Packet.DEFAULT_PAYLOAD_SIZE < this.bits_to_send &&
            this.seq_num - this.ack_num < window_size) {
            return true;
        }
        return false;
    }

    public void _SendPacket() {
        var packet = new Packet{
            payload_size=Packet.DEFAULT_PAYLOAD_SIZE,
            dest=this.dest_ip,
            type=PacketType.DATA,
            seq_num=this.seq_num,
            timestamp = eqp.current_time
        };
        double completion_time = eqp.current_time + packet.size/link.rate;
        eqp.Add(completion_time, link.ReceivePacket(packet));
        this.seq_num += 1;
        // if HasPacketsToSend() == false, it will be idempotent
        eqp.Add(completion_time, SendPacket());
        eqp.Add(completion_time + this.roundtrip_est, CheckTimeout(packet));
        hStat.flows[0].time = eqp.current_time;
        hStat.flows[0].window_size = window_size;
        Logger.LogHostStatus(hStat);
    }

    public Event CheckTimeout(Packet packet) {
        return () => {
            if (this.ack_num < packet.seq_num) { // timed out
                this.window_size = Math.Min(1, this.window_size / 2);
                this.seq_num = this.ack_num;
            }
            eqp.Add(eqp.current_time, SendPacket());
        };
    }

    public override string ToString() {
        return string.Format("<HostAIMDSender window_size={0} seq_num={1} ack_num={2}>",
                this.window_size, this.seq_num, this.ack_num);
    }
}

public class HostReceiver : Host {
    public HostReceiver(EventQueueProcessor eqp, string name) : base(eqp, name) {
    }
}

}
