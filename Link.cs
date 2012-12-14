using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace simulator {
/// <summary>
/// Represents a physical link between two nodes, as well as the send buffer for that link.  
/// Handles buffering, packet dropping and link cost calculation.
/// </summary>
public class Link {
    public readonly EventQueueProcessor eqp;
    public readonly Node src;
    public readonly Node dest;
    /// <summary>Link rate, in bit/s</summary>
    public readonly double rate;
    public double cost;
    
    private long prev_delivered_packets = 0;
    private double prev_calc_time = -10000;
    /// <summary>
    /// Recalculates this link's cost dynamically, considering load information over the preceding time period.
    /// </summary>
    /// <returns>
    /// The calculated cost.
    /// </returns>
    public double CalculateCost() {
        cost = buffer.Count * 8 * 1024 / rate + prop_delay;
        Simulator.Message(this.name + "'s new cost = " + cost);
        prev_calc_time = eqp.current_time;
        prev_delivered_packets = lStatus.delivered_packets;
        
        return cost;
    }
    
    public readonly double prop_delay;
    public string name;
    /// <summary>Buffer capacity in bits</summary>
    public Int64 buffer_size;
    /// <summary>Whether the Link is currently transmitting a packet</summary>
    private bool is_transmitting;
    public LinkStatus lStatus;
    public Queue<Packet> buffer;
    /// <value>Current buffer occupancy in bits</value>
    private Int64 buffer_occupancy {
        get {
            return this.buffer.Sum(pkt => (Int64)pkt.size);
        }
    }
    public Link(EventQueueProcessor eqp, string name, Node src, Node dest, double rate, double prop_delay, Int64 buffer_size) {
        this.eqp = eqp;
        this.src = src;
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
                //Simulator.Message(name + ":transmitting " + packet);
            }
            else if (this.buffer_occupancy + packet.size < this.buffer_size)
            {
                this.buffer.Enqueue(packet);
                //Simulator.Message(name + ":queueing " + packet);
            }
            else
            {
                this.lStatus.dropped_packets++;
                Simulator.Message("Link {0}: buffer {1}/{2}; dropping {3}",
                    this.name,
                    this.buffer_occupancy,
                    this.buffer_size,
                    packet);
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
                //Simulator.Message(name + ":transmitting " + nextPkt);
                TransmitPacket(nextPkt);
            }
            this.lStatus.delivered_packets++;
            //Logger.LogLinkStatus(lStatus);
        };
    }
    public override string ToString() {
        return string.Format("<Link name={3} ({4}→{0}) rate={1} prop_delay={2}>", dest, rate, prop_delay, name, src);
    }
}

}
