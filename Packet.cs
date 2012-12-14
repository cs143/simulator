using IP = System.String;

namespace simulator
{

public enum PacketType {
    /// <summary>
    /// TCP acknowledgement packet, sent from the destination of a TCP flow back to the source
    /// </summary>
    ACK,
    /// <summary>
    /// Ordinary TCP data packet.
    /// </summary>
    DATA,
    /// <summary>
    /// Packet containing the new costs for a link. Used to share information needed to build the routing table.
    /// </summary>
    LINK_STATE_ADVERTISEMENT
}

public struct Packet {
    #region Common fields
    /// <summary>Specifies Node that created this packet</summary>
    public IP src;
    public PacketType type;
    /// Sequence number for this packet. Meaning is dependent on the type (TCP or link-state packets).
    public int seq_num;
    /// Timestamp which will be used for roundtrip measurement (for convenience)
    public double timestamp;
    #endregion
    
    #region Fields used by TCP (ACK and DATA packets)
    /// <remarks>Node that packet is destined for</remarks>
    public IP dest;
    #endregion
    
    #region Link state information
    /// <summary>
    /// Reported link cost of the link from src to link_dest as of recalculation seq_num.
    /// </summary>
    public double link_cost;
    /// <summary>Together with src, uniquely specifies the link that this packet describes.</summary>
    public IP link_dest;
    #endregion
    
    #region Packet sizes
    public static readonly int HEADER_SIZE = 0;
    public static readonly int DEFAULT_PAYLOAD_SIZE = 8*1024;
    public static readonly int DEFAULT_ACK_SIZE = 8*64;
    public static readonly int LINK_STATE_ADVERTISEMENT_SIZE = 0 /*make sure routing packets can always fit in buffer => never get dropped*/;
    /// <value>
    /// Total size of packet, including headers and payload, in bits
    /// </value>
    public int size {
        get {
            return payload_size + Packet.HEADER_SIZE;
        }
    }
    public int payload_size;
    #endregion
    
    public override string ToString() {
        if(this.type == PacketType.LINK_STATE_ADVERTISEMENT)
            return string.Format("<Packet type={3} size={0} src={2} link_dest={1} seq_num={4}>", size, link_dest, src, type, seq_num);
        else
            return string.Format("<Packet type={3} size={0} src={2} dest={1} seq_num={4}>", size, dest, src, type, seq_num);
    }
    
    /// <summary>Convenience function to instantiate a link-state advertisement packet.</summary>
    /// <remarks>The new packet is meant to share a newly-calculated link cost. It is assumed the link cost has just been (re)calculated.</remarks>
    /// <param name='src'>The node that is reporting link state.</param>
    /// <param name='link'>
    /// The link that the new packet will describe. Destination and current link cost will be read from the link.
    /// </param>
    /// <param name='seq_num'>
    /// The new packet will represent the link's status for the <paramref name="seq_num"/>th routing calculation.
    /// </param>
    public static Packet CreateLinkStateAdvertisement(int seq_num, Node src, Link link, double current_time) {
        return new Packet{
            payload_size=Packet.LINK_STATE_ADVERTISEMENT_SIZE,
            src=src.ip,
            link_dest=link.dest.ip,
            type=PacketType.LINK_STATE_ADVERTISEMENT,
            seq_num=seq_num,
            timestamp=current_time
        };
    }
}

}
