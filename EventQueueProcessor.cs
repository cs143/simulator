using System.Collections.Generic;
using System.Linq;

using Time = System.Double;

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
	public Time current_time { get; protected set; }
	private SortedList<Time, Event> queue = new SortedList<Time, Event>();
	public void Add(Time time, Event evt)
	{
		queue.Add(time, evt);
	}
	/** Runs the events in the event queue, including new events created, until the queue is empty. */
	public void Execute()
	{
		while(queue.Count > 0)
		{
			var next = queue.First<KeyValuePair<Time, Event>>();
			queue.RemoveAt(0);
			current_time = next.Key;
			Event next_event = next.Value;
			next_event();
		}
	}
}

public delegate void Event();