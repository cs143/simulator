using System;

namespace simulator {

public class Link {
    public readonly EventQueueProcessor eqp;
    public readonly Host dest;
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
    public LinkStatus lStatus = new LinkStatus();
    public Link(EventQueueProcessor eqp, string name, Host dest, double rate, double prop_delay, Int64 buffer_size) {
        this.eqp = eqp;
        this.dest = dest;
        this.rate = rate;
        this.name = name;
        this.prop_delay = prop_delay;
        this.buffer_size = buffer_size;
        this.lStatus.link_name = name;
        this.lStatus.dropped_packets = 0;
        this.lStatus.buffer_size = 0;
        this.lStatus.delivered_packets = 0;
    }
    
    /// <summary>Stores the packet in the send buffer (or discards it if the buffer is full).<c/summary>
    /// <remarks>
    /// Note: Not an event, because a Link can always-immediately accept a packet into the queue.
    /// The packet will be sent asynchronously at some later time.
    /// </remarks>
    /// <param name='packet'>
    /// The packet to be sent along this link
    /// </param>
    public void EnqueuePacket(Packet packet) {
        //TODO
        double completion_time = eqp.current_time + packet.size/this.rate;
        eqp.Add(completion_time, dest.ReceivePacket(packet));
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// The event where the Link finishes receiving a packet.
    /// </summary>
    // TODO delete; this is wrong
    public Event ReceivePacket(Packet packet) {
        return () => {
            eqp.Add(eqp.current_time+prop_delay, dest.ReceivePacket(packet));
            this.lStatus.delivered_packets++;
            this.lStatus.time = eqp.current_time;
            Logger.LogLinkStatus(lStatus);
        };
    }

    public override string ToString() {
        return string.Format("<Link dest={0} rate={1} prop_delay={2}>", dest, rate, prop_delay);
    }
}

}
