using IP = System.String;

namespace simulator
{

// TCP
public enum PacketType {
    ACK, DATA
}

public struct Packet {
    public int payload_size;
    public IP dest;
    public IP src;
    public PacketType type;
    public int seq_num;
    public double timestamp;// = -1; // Timestamp which will be used for roundtrip measurement (for convenience)

    public static readonly int HEADER_SIZE = 10;
    public static readonly int DEFAULT_PAYLOAD_SIZE = 8*1024;
    public static readonly int DEFAULT_ACK_SIZE = 8*64;

    public int size {
        get {
            return payload_size + Packet.HEADER_SIZE;
        }
    }

    public override string ToString() {
        return string.Format("<Packet size={0} dest={1} src={2} type={3} seq_num={4}>", size, dest, src, type, seq_num);
    }
}

}
