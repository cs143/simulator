using IP = System.Int32;
using System;

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
    private static IP next_ip = 100;
    public readonly IP ip = next_ip++;
    public readonly string name;
    
    protected Node(EventQueueProcessor eqp, string name) {
        this.eqp = eqp;
        this.name = name;
    }
}
}

