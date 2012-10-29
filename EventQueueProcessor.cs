using System.Collections.Generic;

public class Simulator
{
	static void Main()
	{
		var eqp = new EventQueueProcessor();
		eqp.Add(0.0, StupidEvent(0));
		eqp.Execute();
	}
	
	private Event StupidEvent(int x)
	{
		return () => {
			SimulatorLog.add("Hello, world!" + x);
			System.Console.WriteLine("Hello, world!" + x);
			return new Dictionary(){{x + 1, StupidEvent(x + 1)}};
		};
	}
}

public class EventQueueProcessor
{
	public double current_time { public get; protected set; }
	private var queue = new SortedDictionary<Double, Event>();
	public void Add(double time, Event evt)
	{
		queue.add(time, evt);
	}
	/** Runs the events in the event queue, including new events created, until the queue is empty. */
	public void Execute()
	{
		while(queue.Count > 0)
		{
			// @@@@pseudocode
			(double current_time, Event next_event) = queue.RemoveHead();
			// Convert times in the future to absolute times.
			queue.AddAll(mapAt(0, current_time + _, next_event()));
		}
	}
}

/**
* Function type. When executed, an event returns a dictionary of events that should be executed in the future.
* (Time in the future)
*/
public delegate void /*IDictionary<double, Event>*/ Event();



