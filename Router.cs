using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace simulator
{

public class Router
{
    string Name;
    private readonly IDictionary<Host, Router> routing_table = new Dictionary<Host, Router>();
    public Router(string name)
    {
        this.Name = name;
    }
    
    /// <summary>
    /// Event that router is notified that a packet has been received,
    /// and is now waiting in a link's buffer.
    /// </summary>
    public Event NotifyReceivedPacket(Packet packet) {
        return () => {
            // TODO
        };
    }
}

}
