using IP = System.Int32;
using System;
public class Host {
    private static IP next_ip = 0; // HAX

    public readonly EventQueueProcessor eqp;
    public readonly IP ip = Host.next_ip++;
    public readonly string name;
    public Link link { get; set; }

    public Host(EventQueueProcessor eqp, string name) {
        this.eqp = eqp;
        this.name = name;
    }
    public Event SetupSend(IP to_ip, Int64 bits)
    {
        return () => { };
    }
    public Event SendPacket() {
        // TODO what to do about seq_num?

        return () => {
            var packet = new Packet{payload_size=100,
                dest=link.dest.ip,
                type=PacketType.DATA,
                seq_num=0};
            eqp.Add(eqp.current_time + packet.size/link.rate,
                    link.ReceivePacket(packet));
        };
    }

    public Event ReceivePacket(Packet packet) {
        return () => {
            if (packet.type == PacketType.ACK) {
                // TODO process ACK packet
            } else {
                System.Console.WriteLine("Hello, packet!" + packet);
            }
        };
    }

    public Packet CreateAckPacket(IP dest) {
        // TODO account for dropped packets
        return new Packet{payload_size=0, src=this.ip,
                          dest=dest, type=PacketType.ACK};
    }

    public override string ToString() {
        return string.Format("<Host ip={0} name={1}>", ip, name);
    }
}
