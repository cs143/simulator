public class NodeFactory {
    public static NodeFactory FromConfig() {
        return new NodeFactory();
    }

    public Link CreateLink(EventQueueProcessor eqp, Host h1, Host h2) {
        var link = new Link(eqp, h2, System.Math.Pow(10, 6), 0.000010);
        h1.link = link;
        return link;
    }
}

public class Simulator
{
	static EventQueueProcessor eqp = new EventQueueProcessor();
    public static readonly int BITS_TO_SEND= 20*8*1024*1024; // 20MB

	static void Main()
	{
        var factory = NodeFactory.FromConfig();
        var host1 = new HostAIMDSender(eqp, "kijun");
        var host2 = new HostReceiver(eqp, "mike");
        factory.CreateLink(eqp, host1, host2);
        factory.CreateLink(eqp, host2, host1);
        host1.SetupSend(host2.ip, BITS_TO_SEND);
		eqp.Execute();
	}
}
