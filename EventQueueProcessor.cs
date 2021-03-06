using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System;
using Time = System.Double;

namespace simulator {

public class EventQueueProcessor
{
    /// <value>
    /// Current time in seconds
    /// </value>
    public Time current_time { get; protected set; }
    public Time total_time = 0;
    private PriorityQueue<Time, Event> queue = new PriorityQueue<Time, Event>();
    public void Add(Time time, Event evt)
    {
        queue.Enqueue(time, evt);
    }

    /** Runs the events in the event queue, including new events created, until the queue is empty. */
    public void Execute()
    {
        while((queue.Count > 0) && (!IsDone()))
        {
            var next = queue.DequeuePriorityItem();
            if ((this.total_time ==0) || (next.Priority < this.total_time))
            {
                current_time = next.Priority;
                Event next_event = next.Data;
                next_event();
            }
        }
    }
    private bool IsDone()
    {
        return (this.total_time > 0) && (this.current_time > this.total_time); 
    }
}

public delegate void Event();

}
