using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace simulator
{
    public class DumbRouter:DumbNode
    {
        public Link reverse_link;
        public DumbRouter(string name)
        {
            this.name = name;
        }
        public override Event ReceivePacket(Packet pkt)
        {
            return () =>
                {
                    if (pkt.type != PacketType.ACK)
                    {
                        Simulator.eqp.Add(Simulator.eqp.current_time, this.link.ReceivePacket(pkt));
                    }
                    else
                    {
                        Simulator.eqp.Add(Simulator.eqp.current_time, this.reverse_link.ReceivePacket(pkt));
                    }
                };
        }
    }
}
