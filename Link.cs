using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace simulator {

public class Link {
    public readonly EventQueueProcessor eqp;
    public readonly Node dest;
    public readonly double rate;
    //public double cost;

    public double cost {
        get {
            return 1.0 / rate;
        }
    }

    /*
    private int prev_delivered_packets = 0;
    private double prev_calc_time = -10000;

    public Event CalculateCost () {
        return () => {
            cost = (lStatus.delivered_packets - prev_delivered_packets) / System.Math.Abs(eqp.current_time - prev_calc_time);
            cost = System.Math.Min(cost,1);// + prop_delay;
            prev_calc_time = eqp.current_time;
            prev_delivered_packets = lStatus.delivered_packets;
        };
    }
    */

    public readonly double prop_delay;
    public string name;
    public Int64 buffer_size;
    /// <summary>Whether the Link is currently transmitting a packet</summary>
    private bool is_transmitting;
    public LinkStatus lStatus;
    public Queue<Packet> buffer;
    public Link(EventQueueProcessor eqp, string name, Node dest, double rate, double prop_delay, Int64 buffer_size) {
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
        this.is_transmitting = false;
        this.buffer = new Queue<Packet>();
    }

    /// <summary>
    /// The event where the Link stores the packet in the send buffer (or discards it if the buffer is full).
    /// </summary>
    /// <remarks>
    /// Note: A Link always immediately either accept a packet into the queue or discards it.
    /// The packet will be sent asynchronously at some later time.
    /// </remarks>
    /// <param name='packet'>
    /// The packet to be sent along this link
    /// </param>
    public Event EnqueuePacket(Packet packet) {
        return () => {
            if (!this.is_transmitting)
            {
                TransmitPacket(packet);
                //Console.WriteLine(name + ":transmitting " + packet);
            }
            else if (this.buffer.Count < this.buffer_size)
            {
                this.buffer.Enqueue(packet);
                //Console.WriteLine(name + ":queueing " + packet);
            }
            else
            {
                this.lStatus.dropped_packets++;
                Console.WriteLine("dropping " + packet);
            }
            //Logger.LogLinkStatus(lStatus);
        };
    }
    private void TransmitPacket(Packet packet)
    {
        Debug.Assert(!this.is_transmitting, "Bug: Tried to transmit two packets simultaneously");
        this.is_transmitting = true;
        double trans_duration = packet.size / this.rate;
        eqp.Add(eqp.current_time + trans_duration, this.PacketTransmissionComplete());
        double arrival_time = eqp.current_time + trans_duration + prop_delay;
        eqp.Add(arrival_time, dest.ReceivePacket(packet));
    }
    /// <summary>
    /// Event fired when the Link has finished transmitting a packet. The Link can then
    /// process the next packet in the queue, if any.
    /// </summary>
    private Event PacketTransmissionComplete()
    {
        return () => {
            this.is_transmitting = false;
            if (this.buffer.Count > 0)
            {
                Packet nextPkt = this.buffer.Dequeue();
                //Console.WriteLine(name + ":transmitting " + nextPkt);
                TransmitPacket(nextPkt);
            }
            this.lStatus.delivered_packets++;
            //Logger.LogLinkStatus(lStatus);
        };
    }
    public override string ToString() {
        return string.Format("<Link dest={0} rate={1} prop_delay={2}>", dest, rate, prop_delay);
    }

}

}
