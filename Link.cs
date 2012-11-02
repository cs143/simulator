public class Link {
    public readonly EventQueueProcessor eqp;
    public readonly Host dest;
    public readonly double rate;
    public readonly double prop_delay;

    public Link(EventQueueProcessor eqp, Host dest, double rate, double prop_delay) {
        this.eqp = eqp;
        this.dest = dest;
        this.rate = rate;
        this.prop_delay = prop_delay;
    }

    public Event ReceivePacket(Packet packet) {
        return () => {
            eqp.Add(eqp.current_time+prop_delay, dest.ReceivePacket(packet));
        };
    }

    public override string ToString() {
        return string.Format("<Link dest={0} rate={1} prop_delay={2}>", dest, rate, prop_delay);
    }
}
