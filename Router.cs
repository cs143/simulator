/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;

using IP = System.String;

namespace simulator
{

public class Router : Node
{
    /// <summary>
    /// Routing table: maps a destination to the next hop for packet destined therefor.
    /// Values will be null iff the destination is not reachable from here.
    /// </summary>
    private IDictionary<Node, Node> routing_table;
    public Router(EventQueueProcessor eqp, IP ip) : base(eqp, ip) {
    }
    
    /// <summary>
    /// Event that router receives a packet.
    /// The router will immediately perform a routing-table lookup, and forward the packet along the appropriate link.
    /// </summary>
    public override Event ReceivePacket(Packet packet) {
        return () => {
            Node next = routing_table[Simulator.Nodes[packet.dest]];
            Link to_next = Simulator.LinksBySrcDest[Tuple.Create((Node)this, next)];
            to_next.EnqueuePacket(packet);
        };
    }
    
    
    /// <summary>
    /// Calculates the shortest path to every other node in the network.
    /// The implementation uses Dijkstra's algorithm, for use in a link-state routing protocol.
    /// </summary>
    private IDictionary<Node, Node> CalculateShortestPaths() {
        var nodes = Simulator.Nodes.Values;	// TODO all nodes rather than hosts
        /// Best known distance (sum of costs) from this to each node
        IDictionary<Node, double> dist = nodes.ToDictionary(node => node, _ => Double.PositiveInfinity);
        /// Predecessor of each node in shortest-paths tree
        IDictionary<Node, Node> previous = nodes.ToDictionary(node => node, _ => (Node)null);
        
        dist[this] = 0;
        /// Nodes that have yet to be processed
        ISet<Node> queue = new HashSet<Node>(nodes); // TODO Priority queue would be faster; replace if needed
        
        while(queue.Count > 0) {
            Node u = queue.MinBy(node => dist[node]);
            queue.Remove(u);
            if(dist[u] == Double.PositiveInfinity)
                break; // remaining nodes are inaccessible
            
            foreach(Node v in queue
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
    private Node FirstHopOnShortestPath(Node dest, IDictionary<Node, Node> shortest_paths_tree) {
        if(dest == this) // shouldn't call this
            throw new ArgumentException("There is no next hop for a packet destined for this router.", "this");
        Node predecessor = shortest_paths_tree[dest];
        return predecessor == this ? dest : FirstHopOnShortestPath(predecessor, shortest_paths_tree);
    }
    
    /// <summary>
    /// Recalculates the routing table.
    /// </summary>
    private void RecalculateRoutingTable() {
        var shortest_paths_tree = CalculateShortestPaths();
        routing_table = (IDictionary<Node, Node>)shortest_paths_tree.Values
            .Where(n => n != this)
            .Select(n => this.FirstHopOnShortestPath(n, shortest_paths_tree));
    }
}

}
*/
