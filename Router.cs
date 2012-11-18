using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// TODO replace with real node class
using Node = simulator.Host;

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
    /// Event that router @@@@@@
    /// </summary>
    public Event ReceivePacket(Packet packet) {
        return () => {
            // TODO
        };
    }
    
    
    /// <summary>
    /// Calculates the shortest path to every other node in the network.
    /// The implementation uses Dijkstra's algorithm, for use in a link-state routing protocol.
    /// </summary>
    private IDictionary<Node, Node?> CalculateShortestPaths() {
        var nodes = Simulator.Hosts.Values;	// TODO all nodes rather than hosts
        /// Best known distance (sum of costs) from this to each node
        IDictionary<Node, double> dist = nodes.ToDictionary(node => node, _ => Double.PositiveInfinity);
        /// Predecessor of each node in shortest-paths tree
        IDictionary<Node, Node?> previous = nodes.ToDictionary(node => node, _ => null);
        
        dist[this] = 0;
        /// Nodes that have yet to be processed
        ISet<Node> queue = new HashSet<Node>(nodes); // TODO Priority queue would be faster; replace if needed
        
        while(queue.Count > 0) {
            Node u = queue.Min(node => dist[node]);
            queue.Remove(u);
            if(dist[nodes] == Double.PositiveInfinity)
                break; // remaining nodes are inaccessible
            
            foreach(Node v /*out-neighbor of u*/in queue
                .Where(v => Simulator.LinksBySrcDest.ContainsKey(Tuple.Create(u, v))))
            {
                Link uv = Simulator.LinksBySrcDest[Tuple.Create(u, v)];
                double new_dist = dist[u] + uv.cost;
                if(new_dist < dist[v]) {
                    dist[v] = new_dist;
                    previous[v] = u;
                    // TODO for priority queue, run queue.DecreaseKey(new_dist, v)
                }
            }
        }
        
        return previous;
    }
    
    private void RecalculateRoutingTable() {
        // TODO
    }
}

}
