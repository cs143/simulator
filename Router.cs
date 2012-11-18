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
    /// <summary>
    /// Routing table: maps a destination to the next hop for packet destined therefor.
    /// </summary>
    private IDictionary<Host, Router> routing_table;
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
    
    /// <returns>
    /// The first hop on the shortest path from <paramref name="this"/> to <paramref name="dest"/> in the <paramref name="shortest_paths_tree"/>
    /// </returns>
    private Node FirstHopOnShortestPath(Node dest, IDictionary<Node, Node?> shortest_paths_tree) {
        if(dest == this)
            throw new ArgumentException("There is no next hop for a packet destined for this router.", "this");
        Node predecessor = shortest_paths_tree[dest];
        return predecessor == this ? dest : FirstHopOnShortestPath(predecessor, shortest_paths_tree);
    }
    
    /// <summary>
    /// Recalculates the routing table.
    /// </summary>
    private void RecalculateRoutingTable() {
        var shortest_paths_tree = CalculateShortestPaths();
        routing_table = shortest_paths_tree
            .Where(n => n != this)
            .Select(n => this.FirstHopOnShortestPath(n, shortest_paths_tree));
    }
}

}
