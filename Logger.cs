using System;
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
            StreamWriter sw = File.AppendText(Simulator.LogFilePath);
            XmlWriter xw = XmlWriter.Create(sw, settings);
            xml.Serialize(xw, entry, ns);
            sw.Close();
        }
    }
    class Logger
    {
        public static void LogHostStatus(HostStatus hStat)
        {
            LogCreator<HostStatus> lc = new LogCreator<HostStatus>();
            lc.LogEntry(hStat);
        }
        public static void LogLinkStatus(LinkStatus lStat)
        {
            LogCreator<LinkStatus> lc = new LogCreator<LinkStatus>();
            lc.LogEntry(lStat);
        }
        public static void InitLogFile()
        {
            string path = Simulator.LogFilePath;
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("<?xml version=\"1.0\" ?>");
                sw.WriteLine("<Logging>");
                sw.Close();
            }	

        }
        public static void CloseLogFile()
        {
            string path = Simulator.LogFilePath;
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("<TotalTime>100</TotalTime>");
                sw.WriteLine("</Logging>");
                sw.Close();
            }
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
            Int64 dropped_packets=10;
            for (int j = 0; j < 100; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    LinkStatus lStat = new LinkStatus();
                    lStat.link_name = "L" + k;
                    lStat.dropped_packets = k*(dropped_packets++);
                    lStat.time = j;
                    LogLinkStatus(lStat);
                }
            }
        }
    }
    public struct FlowStatus
    {
        [XmlAttribute]
        public string flow_name;
        [XmlAttribute]
        public Int64 window_size;//0 if flow has not started
        [XmlAttribute]
        public Int64 time;
    }
    public struct HostStatus
    {
        [XmlAttribute]
        public string host_name;
        public FlowStatus[] flows;
    }
    public struct LinkStatus
    {
        [XmlAttribute]
        public string link_name;
        [XmlAttribute]
        public Int64 time;
        [XmlElement]
        public Int64 dropped_packets;
    }
}
