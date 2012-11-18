using System.Collections.Generic;
using System.Linq;

using Time = System.Double;

namespace simulator {

public class EventQueueProcessor
{
    public Time current_time { get; protected set; }

    private PriorityQueue<Time, Event> queue = new PriorityQueue<Time, Event>();
    public void Add(Time time, Event evt)
    {
//System.Console.WriteLine("QUEUING: "+ time + " AT " + current_time);
        queue.Enqueue(time, evt);
    }

    /** Runs the events in the event queue, including new events created, until the queue is empty. */
    public void Execute()
    {
        while(queue.Count > 0)
        {
            var next = queue.DequeuePriorityItem();
            current_time = next.Priority;
//System.Console.WriteLine("TIME:"+ current_time);
            Event next_event = next.Data;
            next_event();
        }
    }
}

public delegate void Event();

}
