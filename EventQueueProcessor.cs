using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System;
using Time = System.Double;

namespace simulator {

public class EventQueueProcessor
{
    public Time current_time { get; protected set; }

    private PriorityQueue<Time, Event> queue = new PriorityQueue<Time, Event>();
    public void Add(Time time, Event evt)
    {
        queue.Enqueue(time, evt);
    }

    /** Runs the events in the event queue, including new events created, until the queue is empty. */
    public void Execute()
    {
        while(queue.Count > 0)
        {
            var next = queue.DequeuePriorityItem();
            current_time = next.Priority;
            Event next_event = next.Data;
            next_event();
            Console.WriteLine("time " + current_time);
        }
    }
}

public delegate void Event();

}
