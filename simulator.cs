using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;
using System.Linq;
//using System.Windows.Forms;
using System.ComponentModel;

using IP = System.String;

/*public class NodeFactory {
    public static NodeFactory FromConfig() {
        return new NodeFactory();
    }

    public Link CreateLink(EventQueueProcessor eqp, Host h1, Host h2) {
        var link = new Link(eqp, h2, System.Math.Pow(10, 6), 0.000010);
        h1.link = link;
        return link;
    }
}*/
namespace simulator
{

    public class Simulator
    {
        public static EventQueueProcessor eqp = new EventQueueProcessor();
        public static Dictionary<IP, Host> Hosts;
        public static Dictionary<string, Link> Links;
        public static Dictionary<Tuple<Node, Node>, Link> LinksBySrcDest;
        public static Dictionary<IP, Router> Routers;
        public static Dictionary<IP, Node> Nodes;
        public static string LogFilePath = "";
        static void Main()
        {
            Init();
            /*var factory = NodeFactory.FromConfig();
            var host1 = new Host(eqp, "kijun");
            var host2 = new Host(eqp, "mike");
            factory.CreateLink(eqp, host1, host2);
            factory.CreateLink(eqp, host2, host1);*/

            //eqp.Add(0.0, host1.SendPacket());
            eqp.Execute();
            //Logger.TestLogging();
            Console.WriteLine("Press enter to continue => ");
            Console.ReadLine();
            Logger.CloseLogFile();
        }
        public static void Init()
        {
            XmlDocument xmlDoc = new XmlDocument();
            Console.WriteLine("Enter the path to the configuration file (default = config.xml) =>");
            string fileName = Console.ReadLine();
            if (fileName == "" || fileName == null) fileName = "./config.xml";
            xmlDoc.Load(fileName);
            #region Nodes
            #region Populate Hosts
            Simulator.Hosts = new Dictionary<string, Host>();
            XmlNodeList HostList = xmlDoc.GetElementsByTagName("Host");
            foreach (XmlNode hostNode in HostList)
            {
                string hostName = hostNode.Attributes["name"].Value;
                Console.WriteLine(hostName);
                Simulator.Hosts.Add(hostName, new Host(eqp, hostName));
            }
            #endregion
            #region Populate Routers
            Simulator.Routers = new Dictionary<string, simulator.Router>();
            XmlNodeList router_list = xmlDoc.GetElementsByTagName("Router");
            var routing_info = xmlDoc.GetElementsByTagName("Routing")[0];
            double duration = Convert.ToDouble(routing_info.Attributes["duration"].Value);
            double frequency = Convert.ToDouble(routing_info.Attributes["frequency"].Value);
            foreach (XmlNode router_node in router_list)
            {
                string router_name = router_node.Attributes["name"].Value;
                Console.WriteLine(router_name);
                simulator.Router r = new simulator.Router(eqp, router_name);
                Simulator.Routers.Add(router_name, r);
                // Calculate routing tables at least once before flows start; align to 0
                double build_at = -frequency;
                while (build_at <= duration) {
                    eqp.Add(build_at, r.RecalculateRoutingTableEvent());
                    build_at += frequency;
                }
            }
            #endregion
            // TODO Is there a more elegant way to do this?
            Nodes = Hosts.Select(e => new KeyValuePair<IP, Node>(e.Key, e.Value))
                .Concat(Routers.Select(e => new KeyValuePair<IP, Node>(e.Key, e.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
                // ((IDictionary<IP, Node>)Hosts).Concat<Node>(Routers); // union
            
            //.SelectMany(dict => dict)
              //  .ToDictionary(pair => pair.Key, pair => pair.Value);
            #endregion
            #region Populate Links
            Simulator.Links = new Dictionary<string, Link>();
            Simulator.LinksBySrcDest = new Dictionary<Tuple<Node, Node>, Link>();
            XmlNodeList link_list = xmlDoc.GetElementsByTagName("Link");
            foreach (XmlNode link_node in link_list)
            {
                string link_name = link_node.Attributes["name"].Value;
                string node_to_name = link_node.Attributes["to"].Value;
                string node_from_name = link_node.Attributes["from"].Value;
                Node to_node = Simulator.Nodes[node_to_name];
                Node from_node = Simulator.Nodes[node_from_name];
                Link forward_link = new Link(eqp, link_name, to_node,
                    Convert.ToDouble(link_node.Attributes["rate"].Value),
                    Convert.ToDouble(link_node.Attributes["prop_delay"].Value),
                    Convert.ToInt64(link_node.Attributes["buffer_size"].Value));
                from_node.RegisterLink(forward_link);
                Link reverse_link = new Link(eqp, link_name + "_Reverse", from_node,
                    Convert.ToDouble(link_node.Attributes["rate"].Value),
                    Convert.ToDouble(link_node.Attributes["prop_delay"].Value),
                    Convert.ToInt64(link_node.Attributes["buffer_size"].Value));
                to_node.RegisterLink(reverse_link);
                
                Simulator.Links.Add(forward_link.name, forward_link);
                Simulator.Links.Add(reverse_link.name, reverse_link);
                Simulator.LinksBySrcDest.Add(new Tuple<Node, Node>(from_node, to_node), forward_link);
                Simulator.LinksBySrcDest.Add(new Tuple<Node, Node>(to_node, from_node), reverse_link);

                // events for dynamic cost calculation
                double calc_at = -frequency/5;
                while (calc_at <= duration) {
                    eqp.Add(calc_at, forward_link.CalculateCost());
                    eqp.Add(calc_at, reverse_link.CalculateCost());
                    calc_at += frequency/5;
                }
                
                Console.WriteLine(link_name);
            }
            #endregion
            #region Populate Flows
            XmlNodeList flow_list = xmlDoc.GetElementsByTagName("Flow");
            foreach (XmlNode flow_node in flow_list)
            {
                string flow_name = flow_node.Attributes["name"].Value;
                Host flow_from_host = Simulator.Hosts[flow_node.Attributes["from"].Value];
                Host flow_to_host = Simulator.Hosts[flow_node.Attributes["to"].Value];
                eqp.Add(Convert.ToDouble(flow_node.Attributes["start_time"].Value),
                flow_from_host.SetupSend(flow_to_host.ip, Convert.ToInt64(flow_node.Attributes["pkt_count"].Value), flow_node.Attributes["algorithm"].Value));
                flow_from_host.hStat.flows[0].flow_name = flow_name;
                flow_to_host.flow_rec_stat.flow_name = flow_name;
                Console.WriteLine(flow_name);
            }
            #endregion
            
            LogFilePath = xmlDoc.GetElementsByTagName("LogFilePath")[0].Attributes["path"].Value;
            Console.WriteLine("Log File Path = " + LogFilePath);
            Logger.InitLogFile();
            Console.WriteLine("Press enter to continue => ");
            Console.ReadLine();

        }
        private static Event StupidEvent(int x)
        {
            return () =>
            {
                //SimulatorLog.add("Hello, world!" + x);
                System.Console.WriteLine("Hello, world!" + x);
                eqp.Add(eqp.current_time + 1.0, StupidEvent(x + 1));
            };
        }
    }
}
