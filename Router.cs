using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;

using IP = System.String;

namespace simulator
{

/// <summary>
/// Represents a router that maintains a routing table,
/// and forwards incoming packets to the next hop on the calculated shortest path to the destination
/// specified by the packet.
/// </summary>
public class Router : Node
{
    /// <summary>
    /// Routing table: maps a destination to the next hop for packet destined therefor.
    /// Values will be null iff the destination is not reachable from here.
    /// </summary>
    private IDictionary<Node, Node> routing_table;
    public Router(EventQueueProcessor eqp, IP ip) : base(eqp, ip) {
    }
    
    public override void RegisterLink(Link link) {
        // Nothing to do, since we look up links from the global Simulator.LinksBySrcDest when needed.
    }
    
    /// <summary>
    /// Event that router receives a packet.
    /// The router will immediately perform a routing-table lookup, and forward the packet along the appropriate link.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Throws an exception if the routing table has not been built yet.
    /// You should execute the RecalculateRoutingTableEvent event first.
    /// </exception>
    public override Event ReceivePacket(Packet packet) {
        return () => {
            if(routing_table == null)
                throw new InvalidOperationException("Cannot route packets before routing table is calculated");
            //Simulator.Message(packet);
            Node next = routing_table[Simulator.Nodes[packet.dest]];
            Link to_next = Simulator.LinksBySrcDest[Tuple.Create((Node)this, next)];
            ///Simulator.Message(ip + ":sending " + to_next + packet);
            eqp.Add(eqp.current_time, to_next.EnqueuePacket(packet));
        };
    }
    
    
    /// <summary>
    /// Calculates the shortest path to every other node in the network.
    /// The implementation uses Dijkstra's algorithm, for use in a link-state routing protocol.
    /// </summary>
    /// <returns>
    /// The tree, in the form of a dictionary (node => predecessor of node in shortest path to node), or (this => null).
    /// </returns>
    private IDictionary<Node, Node> CalculateShortestPaths() {
        var nodes = Simulator.Nodes.Values;
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
        var other_nodes = Simulator.Nodes.Values
            .Where(n => n != this);
        routing_table = other_nodes.ToDictionary(n => n, n => FirstHopOnShortestPath(n, shortest_paths_tree));
        Simulator.Message("{0}'s new routing table: {1}",
            this.ip,
            routing_table.Select(kv => string.Format("→{0}:{1}", kv.Key.ip, kv.Value.ip)).ToDelimitedString(", ")
        );
    }
    /// <returns>
    /// Event that tells this Router to recalculate its routing table.
    /// This event should execute at least once prior to any packets being routed.
    /// </returns>
    public Event RecalculateRoutingTableEvent() {
        return () => { this.RecalculateRoutingTable(); };
    }
    
    public override string ToString()
    {
        return string.Format("<router ip={0}>", this.ip);
    }
}

}
