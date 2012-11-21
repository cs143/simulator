using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IP = System.Int32;

namespace simulator
{
    public abstract class DumbNode
    {
        private static IP next_ip = 100;
        public readonly IP ip = DumbNode.next_ip++;
        public abstract Event ReceivePacket(Packet packet);
        public Link link {get;set;}
        public string name = "";
        
    }
}
