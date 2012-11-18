using System;
using System.Collections.Generic; 
 
namespace simulator
{

public class PriorityQueue<P, T>
    where P : System.IComparable<P>
{
    protected List<Queue<PriorityItem<P, T>>> _queues;
    protected int _count; 

    public PriorityQueue()
    {
        _queues = new List<Queue<PriorityItem<P, T>>>();
        _count = 0;
    } 

    /// <summary>
    /// Add an item to the priority queue
    /// </summary>
    public void Enqueue(P priority, T data)
    {
        if (_count == 0)
        {
            Queue<PriorityItem<P, T>> NewQueue = new Queue<PriorityItem<P, T>>();
            NewQueue.Enqueue(new PriorityItem<P, T>(priority, data));
            _queues.Add(NewQueue);
        }
        else
        {
            QueueInsert(priority, data, 0, _queues.Count - 1);
        } 

        _count++;
    } 

    /// <summary>
    /// Helper method for Enqueue
    /// </summary>
    private void QueueInsert(P priority, T data, int qLo, int qHi)
    {
        if (qLo == qHi)
        {
            // There is only one item left to compare.
            // Need to decide where this item belongs in relation to the last item.
            if (_queues[qLo].Peek().Priority.CompareTo(priority) < 0)
            {
                Queue<PriorityItem<P, T>> NewQueue = new Queue<PriorityItem<P, T>>();
                NewQueue.Enqueue(new PriorityItem<P, T>(priority, data));
                _queues.Insert(qLo+1, NewQueue);
                return;
            }
            else if (_queues[qLo].Peek().Priority.CompareTo(priority) > 0)
            {
                Queue<PriorityItem<P, T>> NewQueue = new Queue<PriorityItem<P, T>>();
                NewQueue.Enqueue(new PriorityItem<P, T>(priority, data));
                _queues.Insert(qLo, NewQueue);
                return;
            }
            else
            {
                _queues[qLo].Enqueue(new PriorityItem<P, T>(priority, data));
                return;
            }
        }
        else
        {
            // Get the middle item from the queue and see if we
            // need to go to the first or second half of the queues list
            int qMid = Convert.ToInt32(Math.Floor((qLo + qHi) / 2.0));
            if (_queues[qMid].Peek().Priority.CompareTo(priority) < 0)
            {
                // This item belongs in the upper half of the range
                QueueInsert(priority, data, qMid+1, qHi);
                return;
            }
            else if (_queues[qMid].Peek().Priority.CompareTo(priority) > 0)
            {
                // This item belongs in the lower half of the range
                QueueInsert(priority, data, qLo, qMid);
                return;
            }
            else
            {
                // we got lucky, the middle item is of the same priority
                _queues[qMid].Enqueue(new PriorityItem<P, T>(priority, data));
                return;
            }
        }
    } 

    /// <summary>
    /// Remove the top item from the queue
    /// </summary>
    public T Dequeue()
    {
        if (_queues.Count == 0)
        {
            // There are no items in the priority queue
            return default(T);
        } 

        // Get the first item from the first queue
        T data = _queues[0].Dequeue().Data; 

        if (_queues[0].Count == 0)
        {
            // If the queue at the top priority is empty, remove it
            _queues.RemoveAt(0);
        } 

        _count--; 

        return data; 

    } 

    /// <summary>
    /// </summary>
    public PriorityItem<P, T> DequeuePriorityItem()
    {
        if (_queues.Count == 0)
        {
            // There are no items in the priority queue
            return null;
        } 

        // Get the first item from the first queue
        PriorityItem<P, T> item = _queues[0].Dequeue(); 

        if (_queues[0].Count == 0)
        {
            // If the queue at the top priority is empty, remove it
            _queues.RemoveAt(0);
        } 

        _count--; 

        return item;
    } 

    /// <summary>
    /// Retrieves the top item from the queue without removing it
    /// </summary>
    public T Peek()
    {
        if (_queues.Count > 0)
        {
              return _queues[0].Peek().Data;
        }
        else return default(T);
    } 

    /// <summary>
    /// Gets the number of items in the priority queue
    /// </summary>
    public int Count
    {
        get
        {
            return _count;
        }
    } 

    /// <summary>
    /// Returns a string representation of the queue
    /// </summary>
    public override string ToString()
    {
        string val = string.Empty;
        foreach(Queue<PriorityItem<P, T>> queue in _queues)
        {
            PriorityItem<P, T>[] items = queue.ToArray();
            foreach (PriorityItem<P, T> item in items)
            {
                val += string.Format(" [ {0} ] ", item.Data.ToString());
            }
        }
        return val.TrimEnd(',');
    }
} 

public class PriorityItem<P, T>
    where P : System.IComparable<P>
{
    public P Priority;
    public T Data; 

    public PriorityItem(P priority, T data)
    {
        this.Priority = priority;
        this.Data = data;
    }
}

}
