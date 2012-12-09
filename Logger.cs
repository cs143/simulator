﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
namespace simulator
{
    public class LogCreator<EntryType>
    {
        public void LogEntry(EntryType entry)
        {
            XmlSerializer xml = new XmlSerializer(typeof(EntryType));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //  Add lib namespace with empty prefix
            ns.Add("", "");
            XmlWriter xw = XmlWriter.Create(Logger.asw, settings);
            xml.Serialize(xw, entry, ns);
        }
    }
    class Logger
    {
        public static StreamWriter asw;
        public static void LogHostStatus(HostStatus hStat)
        {
            if (hStat.flows.Length == 0) return;
            if (hStat.flows[0].flow_name == "") return;
            LogCreator<HostStatus> lc = new LogCreator<HostStatus>();
            lc.LogEntry(hStat);
        }
        public static void LogLinkStatus(LinkStatus lStat)
        {
            LogCreator<LinkStatus> lc = new LogCreator<LinkStatus>();
            lc.LogEntry(lStat);
        }
        public static void LogFlowRecStatus(FlowReceive frStat)
        {
            if (frStat.flow_name == "") return;
            LogCreator<FlowReceive> lc = new LogCreator<FlowReceive>();
            lc.LogEntry(frStat);
        }
        public static void InitLogFile()
        {
            string path = Simulator.LogFilePath;
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("<?xml version=\"1.0\" ?>");
                sw.WriteLine("<Logging>");
                sw.Close();
                asw = File.AppendText(path);
            }	

        }
        public static void CloseLogFile()
        {
            
                asw.WriteLine("<TotalTime>"+Simulator.eqp.current_time+"</TotalTime>");
                asw.WriteLine("</Logging>");
                asw.Close();
        }
        public static Event LogEverything()
        {
            return () =>
            {
                foreach (Node n in Simulator.Nodes.Values)
                {
                    if (n.GetType() == typeof(Host))
                    {
                        Host h = (Host)n;
                        Logger.LogHostStatus(h.hStat);
                        Logger.LogFlowRecStatus(h.flow_rec_stat);
                    }
                }
                foreach (Link l in Simulator.Links.Values)
                {
                    Logger.LogLinkStatus(l.lStatus);
                }
            };
        }
        public static void TestLogging()
        {
            Random pointless = new Random();
            for (int j = 0; j < 100; j++)
            {
                HostStatus hStat = new HostStatus();
                hStat.host_name = "OnlyHost";
                hStat.flows = new FlowStatus[6];
                for (int k = 0; k < 6; k++)
                {
                    FlowStatus fStat = new FlowStatus();
                    fStat.time = j;
                    fStat.window_size = (long)Math.Round( (Math.Log10(j+1)+k) * 100);
                    fStat.flow_name = "Flow" + (k + 1);
                    hStat.flows[k] = fStat;
                }
                LogHostStatus(hStat);
            }
            Int64 dropped_packets=1;
            for (int j = 1; j < 101; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    //LinkStatus lStat = new LinkStatus();
                    //lStat.link_name = "L" + (k+1);
                    //lStat.dropped_packets = k*dropped_packets;
                    //lStat.buffer_size = k * dropped_packets;
                    //lStat.delivered_packets = k * dropped_packets;
                    //lStat.time = j;
                    //LogLinkStatus(lStat);
                    FlowReceive frStat = new FlowReceive();
                    frStat.flow_name = "Flow" + (k + 1);
                    frStat.time = j;
                    frStat.received_packets = k * dropped_packets;
                    LogFlowRecStatus(frStat);
                }
                dropped_packets++;
            }
        }
    }
    public struct FlowStatus
    {
        [XmlAttribute]
        public string flow_name;
        [XmlAttribute]
        public double window_size;//0 if flow has not started
        [XmlAttribute]
        public double time
        {
            get
            {
                return Simulator.eqp.current_time;
            }
            set { }
        }
    }
    public struct FlowReceive
    {
        [XmlAttribute]
        public string flow_name;
        [XmlAttribute]
        public double time
        {
            get
            {
                return Simulator.eqp.current_time;
            }
            set { }
        }
        [XmlElement]
        public Int64 received_packets;
        [XmlAttribute]
        public double packet_delay;
    }

    public struct HostStatus
    {
        [XmlAttribute]
        public string host_name;
        public FlowStatus[] flows;
    }
    public class LinkStatus
    {
        [XmlIgnore]
        public Link link;
        [XmlAttribute]
        public string link_name
        {
            get
            {
                return link.name;
            }
            set{}
        }
        [XmlAttribute]
        public double time
        {
            get
            {
                return link.eqp.current_time;
            }
            set { }
        }
        [XmlElement]
        public Int64 dropped_packets;
        [XmlElement]
        public Int64 buffer_occupancy
        {
            get
            {
                return this.link.buffer.Count;
            }
            set { }
        }
        [XmlElement]
        public Int64 delivered_packets;
    }
}
