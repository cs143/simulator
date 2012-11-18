using System;

namespace simulator {

public class Link {
    public readonly EventQueueProcessor eqp;
    public readonly Host dest;
    public readonly double rate;
    public double cost {
        get {
            return 1.0 / rate;
        }
    }
    public readonly double prop_delay;
    public string name;
    public Int64 buffer_size;
    public Link(EventQueueProcessor eqp, string name, Host dest, double rate, double prop_delay, Int64 buffer_size) {
        this.eqp = eqp;
        this.dest = dest;
        this.rate = rate;
        this.name = name;
        this.prop_delay = prop_delay;
        this.buffer_size = buffer_size;
    }
    
    /// <summary>
    /// The event where the Link finishes receiving a packet.
    /// </summary>
    public Event ReceivePacket(Packet packet) {
        return () => {
            eqp.Add(eqp.current_time+prop_delay, dest.ReceivePacket(packet));
        };
    }

    public override string ToString() {
        return string.Format("<Link dest={0} rate={1} prop_delay={2}>", dest, rate, prop_delay);
    }
}

}