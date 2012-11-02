using IP = System.Int32;
// TCP
public enum PacketType {
    ACK, DATA
}

public struct Packet {
    public int payload_size;
    public IP dest, src;
    public PacketType type;
    public int seq_num;

    public static readonly int HEADER_SIZE = 10;

    public int size {
        get {
            return payload_size + Packet.HEADER_SIZE;
        }
    }

    public override string ToString() {
        return string.Format("<Packet size={0} dest={1} src={2} type={3} seq_num={4}>", size, dest, src, type, seq_num);
    }
}
