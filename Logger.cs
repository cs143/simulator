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
            
                asw.WriteLine("<TotalTime>100</TotalTime>");
                asw.WriteLine("</Logging>");
                asw.Close();
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
                    LinkStatus lStat = new LinkStatus();
                    lStat.link_name = "L" + (k+1);
                    lStat.dropped_packets = k*dropped_packets;
                    lStat.buffer_size = k * dropped_packets;
                    lStat.delivered_packets = k * dropped_packets;
                    lStat.time = j;
                    LogLinkStatus(lStat);
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
        public Int64 window_size;//0 if flow has not started
        [XmlAttribute]
        public Int64 time;
    }
    public struct FlowReceive
    {
        [XmlAttribute]
        public string flow_name;
        [XmlAttribute]
        public Int64 time;
        [XmlElement]
        public Int64 received_packets;
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
        [XmlElement]
        public Int64 buffer_size;
        [XmlElement]
        public Int64 delivered_packets;
    }
}
