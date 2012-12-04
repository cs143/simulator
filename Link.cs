using System;
using System.Collections.Generic;
namespace simulator {

public class Link {
    public readonly EventQueueProcessor eqp;
    public readonly DumbNode dest;
    public readonly double rate;
    public double cost {
        get {
            // TODO Use better cost calculation
            return 1.0 / rate;
        }
    }

    public readonly double prop_delay;
    public string name;
    public Int64 buffer_size;
    public bool is_busy;
    public LinkStatus lStatus;
    public Queue<Packet> buffer;
    public Link(EventQueueProcessor eqp, string name, DumbNode dest, double rate, double prop_delay, Int64 buffer_size) {
        this.eqp = eqp;
        this.dest = dest;
        this.rate = rate;
        this.name = name;
        this.prop_delay = prop_delay;
        this.buffer_size = buffer_size;
        this.lStatus = new LinkStatus();
        this.lStatus.link = this;
        this.lStatus.dropped_packets = 0;
        this.lStatus.delivered_packets = 0;
        this.is_busy = false;
        this.buffer = new Queue<Packet>();
    }

    /// <summary>
    /// The event where the Link finishes receiving a packet.
    /// </summary>
    public Event ReceivePacket(Packet packet) {
        return () => {
            // Console.WriteLine("received "+ packet);
            if (!this.is_busy)
            {
                TransmitPacket(packet);
                // Console.WriteLine("transmitting " + packet);
            }
            else if (this.buffer.Count < this.buffer_size)
            {
                this.buffer.Enqueue(packet);
                //Console.WriteLine("queueing " + packet);
            }
            else
            {
                this.lStatus.dropped_packets++;
                // Console.WriteLine("dropping " + packet);
            }
            Logger.LogLinkStatus(lStatus);
        };
    }

    public Event PacketTransmissionComplete()
    {
        return () =>
            {
                if (this.buffer.Count == 0)
                {
                    this.is_busy = false;
                }
                else
                {
                    Packet nextPkt = this.buffer.Dequeue();
                    // Console.WriteLine("transmitting " + nextPkt);
                    TransmitPacket(nextPkt);
                }
                this.lStatus.delivered_packets++;
                Logger.LogLinkStatus(lStatus);
                
            };
    }

    public override string ToString() {
        return string.Format("<Link dest={0} rate={1} prop_delay={2}>", dest, rate, prop_delay);
    }

    private void TransmitPacket(Packet packet)
    {
        this.is_busy = true;
        double trans_duration = packet.size / this.rate;
        double arrival_time = eqp.current_time + trans_duration + prop_delay;
        eqp.Add(eqp.current_time + trans_duration, this.PacketTransmissionComplete());
        eqp.Add(arrival_time, dest.ReceivePacket(packet));
    }
}

}
