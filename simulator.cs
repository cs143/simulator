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

	static void Main()
	{
        var factory = NodeFactory.FromConfig();
        var host1 = new Host(eqp, "kijun");
        var host2 = new Host(eqp, "mike");
        factory.CreateLink(eqp, host1, host2);
        factory.CreateLink(eqp, host2, host1);

		eqp.Add(0.0, host1.SendPacket());
		eqp.Execute();
	}

	private static Event StupidEvent(int x)
	{
		return () => {
			//SimulatorLog.add("Hello, world!" + x);
			System.Console.WriteLine("Hello, world!" + x);
			eqp.Add(eqp.current_time + 1.0, StupidEvent(x + 1));
		};
	}
}
