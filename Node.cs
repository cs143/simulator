using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

using IP = System.String;

namespace simulator
{

/// <summary>
/// Represents a network node (router or host).
/// </summary>
public abstract class Node
{
    /// <returns>
    /// Event that occurs when this Node receives a packet.
    /// </returns>
    public abstract Event ReceivePacket(Packet packet);
    
    protected readonly EventQueueProcessor eqp;
    /// <summary>
    /// The name of this node, which also functions as its IP address.
    /// </summary>
    public readonly IP ip;
    
    protected Node(EventQueueProcessor eqp, string ip) {
        this.eqp = eqp;
        this.ip = ip;
    }
    
    /// <summary>
    /// Informs this Node about an outbound link.
    /// </summary>
    public abstract void RegisterLink(Link link);
    
    /// <summary>
    /// Recalculates the state of all links originating at this node,
    /// and shares the information with all neighbors.
    /// </summary>
    /// <param name='seq_num'>
    /// New routing sequence number.
    /// </param>
    public abstract Event RecalculateLinkState(int seq_num);
}

}