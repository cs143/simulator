using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using System.Diagnostics;

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
    
    private readonly ISet<Link> outgoing_links = new HashSet<Link>();
    public override void RegisterLink(Link link) {
        outgoing_links.Add(link);
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
            if(packet.type == PacketType.LINK_STATE_ADVERTISEMENT)
                this.ProcessLinkStateAdvertisement(packet);
            else
                this.ForwardOrdinaryPacket(packet);
        };
    }
    
    #region Link-state sharing
    /// <summary>
    /// How many routing recalculations have been performed.
    /// </summary>
    private int routing_seq_num;
    /// <summary>
    /// Link costs for routing_seq_num that this Router currently knows
    /// </summary>
    private IDictionary<Link, double> known_link_costs;
    /// <summary>
    /// True iff the routing table has been updated to use this round's link costs. False iff it needs to be updated.
    /// This avoids recalculating the routing table each time a redundant link-state packet is received.
    /// </summary>
    private bool routing_table_up_to_date;
    
    /// <summary>
    /// Recalculates the state of all links originating at this node,
    /// and shares the information with all neighbors.
    /// </summary>
    /// <param name='seq_num'>
    /// New routing sequence number.
    /// </param>
    public override Event RecalculateLinkState(int seq_num) {
        return () => {
            Debug.Assert(this.routing_seq_num < seq_num, "Routing sequence numbers must be monotonic.");
            this.routing_seq_num = seq_num;
            this.known_link_costs = new Dictionary<Link, double>(); // Discard old cost info (for previous routing sequence number)
            this.routing_table_up_to_date = false;
            this.link_state_sender = new LinkStatePacketSender(eqp, outgoing_links);
            
            // Recalculate costs for our own links, and record in our table
            foreach(Link l in outgoing_links)
                known_link_costs[l] = l.CalculateCost();
            
            // Packets containing the new info about link costs
            var link_state_packets = outgoing_links
                .Select(l => Packet.CreateLinkStateAdvertisement(seq_num, src: this, link: l, current_time: eqp.current_time));
            // Send all the packets on all outgoing links
            link_state_packets.ForEach(pkt => link_state_sender.Send(pkt));
            
            this.RecalculateRoutingTableIfEnoughInfo();
        };
    }
    
    private LinkStatePacketSender link_state_sender;
    private class LinkStatePacketSender {
        private readonly ISet<Link> outgoing_links;
        private readonly EventQueueProcessor eqp;
        public LinkStatePacketSender(EventQueueProcessor eqp, ISet<Link> outgoing_links) {
            this.eqp = eqp;
            this.outgoing_links = outgoing_links;
        }
        private readonly ISet<Packet> already_seen = new HashSet<Packet>();
        /// <summary>
        /// Sends a new packet on all outgoing links.
        /// If during this round, an identical packet has already been sent, does nothing.
        /// This avoids sending infinite loops of link-state packets.
        /// </summary>
        public void Send(Packet pkt) {
            if(!already_seen.Contains(pkt)) {
                // Simulator.Message("Sending link-state packet {0} on links {1}", pkt, outgoing_links.ToDelimitedString());
                foreach(Link l in outgoing_links)
                    eqp.Add(eqp.current_time, l.EnqueuePacket(pkt));
                already_seen.Add(pkt);
            }
        }
    }
    
    
    /// <summary>Recalculates the routing table if we know the latest link costs for all links, and it's not already been updated on this round. Otherwise does nothing.<c/summary>
    private void RecalculateRoutingTableIfEnoughInfo() {
        if(!routing_table_up_to_date) {
            // Simulator.Message("{0}: known link costs: {1}", this.ip, known_link_costs.ToDelimitedString());
            // Simulator.Message("{0}: not known: {1}", this.ip, Simulator.Links.Values.Except(known_link_costs.Keys).ToDelimitedString());
            if(new HashSet<Link>(known_link_costs.Keys).IsSupersetOf(Simulator.Links.Values))
                RecalculateRoutingTable();
        }
    }
    
    /// <summary>
    /// Processes a received link-state advertisement by adding it to the table of known link costs.
    /// If the received packet gives us enough information, this triggers recalculation of the routing table.
    /// The advertisement will be forwarded on all outgoing links (if not already done).
    /// </summary>
    private void ProcessLinkStateAdvertisement(Packet pkt) {
        // Simulator.Message("Router {0} received link-state packet for link {1}", this.ip, pkt);
        Debug.Assert(pkt.seq_num <= this.routing_seq_num);
        if(pkt.seq_num == this.routing_seq_num) { // packet describes current round
            Link described_link = Simulator.LinksBySrcDest[Tuple.Create(Simulator.Nodes[pkt.src], Simulator.Nodes[pkt.link_dest])];
            Debug.Assert(described_link.cost == pkt.link_cost, "Sanity check failed: received link-state packet claiming incorrect link cost");
            if(known_link_costs.ContainsKey(described_link)) {
                Debug.Assert(known_link_costs[described_link] == pkt.link_cost);
            } else {
                known_link_costs.Add(described_link, pkt.link_cost);
            }
            RecalculateRoutingTableIfEnoughInfo();
            link_state_sender.Send(pkt);
        } else {
            Simulator.Message("Warning: received a link-state advertisement from a previous round {0} (current routing seq num = {1})",
                pkt.seq_num, this.routing_seq_num);
        }
    }
    #endregion
    
    /// <summary>
    /// Immediately performs a routing-table lookup and forwards an ordinary packet on the appropriate link.
    /// </summary>
    private void ForwardOrdinaryPacket(Packet packet) {
        if(routing_table == null)
            throw new InvalidOperationException("Cannot route packet before routing table is calculated: " + packet);
        //Simulator.Message(packet);
        Node next = routing_table[Simulator.Nodes[packet.dest]];
        Link to_next = Simulator.LinksBySrcDest[Tuple.Create((Node)this, next)];
        ///Simulator.Message(ip + ":sending " + to_next + packet);
        eqp.Add(eqp.current_time, to_next.EnqueuePacket(packet));
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
                // Look up link cost from our own local table, instead of statically
                double new_dist = dist[u] + this.known_link_costs[uv];
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
    /// Recalculates the routing table. This should execute at least once prior to any packets being routed.
    /// </summary>
    private void RecalculateRoutingTable() {
        var shortest_paths_tree = CalculateShortestPaths();
        var other_nodes = Simulator.Nodes.Values
            .Where(n => n != this);
        routing_table = other_nodes.ToDictionary(n => n, n => FirstHopOnShortestPath(n, shortest_paths_tree));
        this.routing_table_up_to_date = true;
        Simulator.Message("{0}'s new routing table: {1}",
            this.ip,
            routing_table.Select(kv => string.Format("→{0}:{1}", kv.Key.ip, kv.Value.ip)).ToDelimitedString(", ")
        );
    }
    
    public override string ToString()
    {
        return string.Format("<router ip={0}>", this.ip);
    }
}

}
