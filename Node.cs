using System;

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
    /// <remarks>
    /// The event processes link-state advertisement packets (this processing should be common to all nodes on the network).
    /// When an ordinary packet is received,
    /// the event (synchronously) calls the ProcessReceivedPacket abstract method,
    /// which Hosts and Routers override to implement their specific behavior.
    /// </remarks>
    public Event ReceivePacket(Packet packet) {
        return () => {
            if(packet.type@@@)
        }
    }
    /// <summary>
    /// Processes the received packet.
    /// </summary>
    /// <param name='packet'>
    /// Packet.
    /// </param>
    public abstract void ProcessReceivedPacket(Packet packet);
    
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
}
}

