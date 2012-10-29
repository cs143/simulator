using System.Collections.Generic;
using System.Linq;

public class Simulator
{
	private static EventQueueProcessor eqp = new EventQueueProcessor();
	static void Main()
	{
		eqp.Add(0.0, StupidEvent(0));
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

public class EventQueueProcessor
{
	public double current_time { get; protected set; }
	private SortedList<double, Event> queue = new SortedList<double, Event>();
	public void Add(double time, Event evt)
	{
		queue.Add(time, evt);
	}
	/** Runs the events in the event queue, including new events created, until the queue is empty. */
	public void Execute()
	{
		while(queue.Count > 0)
		{
			var next = queue.First<KeyValuePair<double, Event>>();
			queue.RemoveAt(0);
			current_time = next.Key;
			Event next_event = next.Value;
			next_event();
		}
	}
}

public delegate void Event();