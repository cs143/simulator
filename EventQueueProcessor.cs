using System.Collections.Generic;
using System.Linq;

using Time = System.Double;

public class EventQueueProcessor
{
    public Time current_time { get; protected set; }

    private C5.IPriorityQueue<Tuple<Time, Event>> queue = new C5.IntervalHeap<Tuple<Time, Event>>();
    public void Add(Time time, Event evt)
    {
        System.Diagnostics.Debug.Assert(queue.AllowsDuplicates);
        queue.Add(Tuple.Create(time, evt));
    }
    /** Runs the events in the event queue, including new events created, until the queue is empty. */
    public void Execute()
    {
        while(queue.Count > 0)
        {
            // Execute Event at head of queue
            var next = queue.DeleteMin();
            current_time = next.Item1;
            Event next_event = next.Item2; 
            next_event();
        }
    }
}

public delegate void Event();
