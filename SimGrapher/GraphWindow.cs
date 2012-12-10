using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.IO;
using System.Xml;
using System.Xml.Linq;
namespace SimGrapher
{
    public struct Flow
    {
        public string name;
        public PointPairList plist;
        public PointPairList flow_rate_list;
        public PointPairList send_rate_list;
        public PointPairList packet_delay_list;
    }
    public struct Link
    {
        public string name;
        public PointPairList droppedPacketList;
        public PointPairList buffer_size_list;
        public PointPairList link_rate_list;
    }
    public partial class GraphWindow : Form
    {
        public GraphWindow()
        {
            InitializeComponent();
        }

        private void GraphWindow_Load(object sender, EventArgs e)
        {
            OpenFileDialog oDlg = new OpenFileDialog();
            oDlg.Title = "Select simulator log file";
            oDlg.ShowDialog();
            XDocument xmlDoc =  XDocument.Load(oDlg.FileName);
            Flow[] wSizeList = GetFlowStatusList(xmlDoc);
            CreateWindowSizeChart(zedGraphControl1,wSizeList);
            CreateFlowRateChart(zedGraphControl5, wSizeList);
            CreateSendRateChart(zedGraphControl6, wSizeList);
            CreatePacketDelayChart(zedGraphControl7, wSizeList);
            Link[] linkStatList = GetLinkStatusList(xmlDoc);
            CreateDroppedPacketChart(zedGraphControl2, linkStatList);
            CreateBufferSizeChart(zedGraphControl3, linkStatList);
            CreateLinkRateChart(zedGraphControl4, linkStatList);
        }
       public Link[] GetLinkStatusList(XDocument xmlDoc)
        {
            int actcnt=0;
            var linkList = (from item in xmlDoc.Descendants("LinkStatus")
                            select item.Attribute("link_name").Value
                 ).Distinct();
            foreach (var lame in linkList)
                if (!(lame.IndexOf("Reverse")>0))
                {
                    actcnt++;
                }

            Link[] list = new Link[actcnt];
            int count = 0;
            double TotTime = Convert.ToDouble(xmlDoc.Descendants("TotalTime").First().Value);
            double TimeInterval = TotTime / 100;
            foreach (var link in linkList)
            {
                if (link.IndexOf("Reverse")>0) continue;
                list[count] = new Link();
                list[count].name = link;
                list[count].buffer_size_list = new PointPairList();//GetBufferSizeList(xmlDoc, link);
                list[count].droppedPacketList = new PointPairList();
                list[count].link_rate_list = new PointPairList();
                double lastDpCount = 0.0;
                double lastDelCount = 0.0;
#region oldcode
                /*for (int j = 0; j < 100; j++)
                {

                    double dropped_packet_rate = 0;
                    double link_rate = 0;
                    double startTime = j * TimeInterval;
                    double endTime = (j + 1) * TimeInterval;
                        var dPackItem = from item in xmlDoc.Descendants("LinkStatus")
                                         where item.Attribute("link_name").Value == link
                                         && Convert.ToDouble(item.Attribute("time").Value) <= endTime
                                         && Convert.ToDouble(item.Attribute("time").Value) >= startTime
                                         orderby (double)item.Attribute("time")
                                         select new
                                         {
                                             time = item.Attribute("time").Value,
                                             dp_count = item.Descendants("dropped_packets").First().Value,
                                             lr_count = item.Descendants("delivered_packets").First().Value
                                         };
                    if (dPackItem.Count()>0)
                    {
                        var crappyDPackItem = dPackItem.Last();
                        Int64 realDpCount = Convert.ToInt64(crappyDPackItem.dp_count);
                        dropped_packet_rate = (realDpCount - lastDpCount) / TimeInterval;
                        lastDpCount = realDpCount;
                        Int64 realDelCount = Convert.ToInt64(crappyDPackItem.lr_count);
                        link_rate = (realDelCount - lastDelCount)  / (TimeInterval*125);
                        lastDelCount = realDelCount;
                    }
                   
                }*/
#endregion
                var lstat_item = from item in xmlDoc.Descendants("LinkStatus")
                               where item.Attribute("link_name").Value == link
                               orderby (double)item.Attribute("time")
                               select new
                               {
                                   time = item.Attribute("time"),
                                   dp_count = item.Descendants("dropped_packets").First(),
                                   lr_count = item.Descendants("delivered_packets").First(),
                                   buff_size = Convert.ToDouble(item.Descendants("buffer_occupancy").First().Value)
                               };
                double prev_time = 0.0;
                double point_count = 0.0;
                double buff_tot = 0.0;
                foreach (var next_stat in lstat_item)
                {
                    double x = (double) next_stat.time;
                    double dropped_packet_rate = ((double)next_stat.dp_count - lastDpCount)/((double)next_stat.time - prev_time);
                    double link_rate = ((double)next_stat.lr_count - lastDelCount)/(125*((double)next_stat.time - prev_time));
                    list[count].buffer_size_list.Add(x,(double)next_stat.buff_size);
                    list[count].droppedPacketList.Add(x, dropped_packet_rate);
                    list[count].link_rate_list.Add(x, link_rate);
                    lastDpCount = (double)next_stat.dp_count;
                    lastDelCount = (double)next_stat.lr_count;
                    prev_time = (double)next_stat.time;
                    point_count++;
                    buff_tot += (double)next_stat.buff_size;
                }
                int newCount = dataGridLinks.Rows.Add();
                dataGridLinks.Rows[newCount].Cells[0].Value = link;
                double avgLoss = lastDpCount / prev_time;
                dataGridLinks.Rows[newCount].Cells["AvgLoss"].Value = Math.Round(avgLoss,2);
                double mean_buff_size = buff_tot / point_count;
                dataGridLinks.Rows[newCount].Cells["AvgOccup"].Value =Math.Round(mean_buff_size,2);
                double sum_of_squares = 0.0;
                foreach (var next_stat in lstat_item)
                {
                    double difference = (double)next_stat.buff_size - mean_buff_size;
                    sum_of_squares += difference * difference;
                }
                double var_buff_size = sum_of_squares/point_count;
                dataGridLinks.Rows[newCount].Cells["VarOccup"].Value = Math.Round(var_buff_size, 2);
                count++;
            }
            return list;
        }
        public Flow[] GetFlowStatusList(XDocument xmlDoc)
        {
            var flowList = (from item in xmlDoc.Descendants("FlowStatus")
                            select item.Attribute("flow_name").Value
                            ).Distinct();
                        
            Flow[] list = new Flow[flowList.Count()];
            int count = 0;
            double TotTime = Convert.ToDouble(xmlDoc.Descendants("TotalTime").First().Value);
            double TimeInterval = TotTime / 100;
            foreach (var flow in flowList)
            {
                list[count] = new Flow();
                list[count].name = flow;
                list[count].plist = new PointPairList();
                list[count].send_rate_list = new PointPairList();
                var wSizeItems = from item in xmlDoc.Descendants("FlowStatus")
                                 where item.Attribute("flow_name").Value == flow
                                 orderby (double)item.Attribute("time")
                                 select new
                                 {
                                     time = item.Attribute("time"),
                                     wsize = item.Attribute("window_size"),
                                     sent_packets = item.Attribute("packets_sent")
                                 };
                list[count].flow_rate_list = new PointPairList();
                list[count].packet_delay_list = new PointPairList();
                var rec_item = from item in xmlDoc.Descendants("FlowReceive")
                                 where item.Attribute("flow_name").Value == flow
                                 orderby (double)item.Attribute("time")
                                 select new
                                 {
                                     time = item.Attribute("time"),
                                     rec_count = item.Descendants("received_packets").First(),
                                     avg_delay = item.Attribute("packet_delay")
                                 };

                #region obsolete
                /*Int64 lastRecCount = 0;
                for (int j = 0; j < 100; j++)
                {

                    double flow_rate = 0;
                    double startTime = j * TimeInterval;
                    double endTime = (j + 1) * TimeInterval;
                    var rec_item = from item in xmlDoc.Descendants("FlowReceive")
                                   where item.Attribute("flow_name").Value == flow
                                   && Convert.ToDouble(item.Attribute("time").Value) <= endTime
                                   && Convert.ToDouble(item.Attribute("time").Value) >= startTime
                                   orderby (double)item.Attribute("time")
                                   select new
                                   {
                                       time = item.Attribute("time").Value,
                                       rec_count = item.Descendants("received_packets").First().Value,
                                   };
                    if (rec_item.Count()>0)
                    {
                        var crappyRecItem = rec_item.Last();
                        Int64 realRecCount = Convert.ToInt64(crappyRecItem.rec_count);
                        flow_rate = (realRecCount - lastRecCount)  / TimeInterval;
                        lastRecCount = realRecCount;
                    }
                    list[count].flow_rate_list.Add((j + 1) * TimeInterval, flow_rate);
                }*/
                #endregion
                double prev_time = 0.0;
                double prev_count = 0.0;
                foreach (var pt in wSizeItems)
                {
                    
                    double x = (double)pt.time;
                    double y = (double)pt.wsize;
                    list[count].plist.Add(x, y);
                    double send_rate = ((double)pt.sent_packets - prev_count) / (125*((double)pt.time - prev_time));
                    list[count].send_rate_list.Add(x, send_rate);
                    prev_time = x;
                    prev_count = (double)pt.sent_packets;
                }
                prev_time = 0.0;
                prev_count = 0.0;
                double first_non_zero = -1;
                double last_non_zero = -1;
                double packet_delay_tot = 0.0;
                foreach (var pt2 in rec_item)
                {
                    double x = (double)pt2.time;
                    double y = ((double)pt2.rec_count - prev_count) / (125*((double)pt2.time - prev_time));
                    if (y > 0)
                    {
                        if (first_non_zero == -1) first_non_zero = x;
                        last_non_zero = x;
                    }
                    list[count].flow_rate_list.Add(x, y);
                    list[count].packet_delay_list.Add(x, (double)pt2.avg_delay);
                    prev_time = x;
                    prev_count = (double)pt2.rec_count;
                    packet_delay_tot += (double)pt2.avg_delay;
                }
                int newRowCount = dataGridFlows.Rows.Add();
                dataGridFlows.Rows[newRowCount].Cells[0].Value = flow;
                dataGridFlows.Rows[newRowCount].Cells["AvgThroughput"].Value = Math.Round(prev_count /(125* (last_non_zero - first_non_zero)),2);
                dataGridFlows.Rows[newRowCount].Cells["AvgDelay"].Value = Math.Round(packet_delay_tot /(double)rec_item.Count(), 2);
                count++;
            }
            return list;
        }
        public void CreateWindowSizeChart(ZedGraphControl zgc, Flow[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            ZedGraph.SymbolType[] GraphSymbols = { SymbolType.Circle, SymbolType.Diamond, SymbolType.TriangleDown, SymbolType.Square, SymbolType.Star, SymbolType.Triangle };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, s";
            myPane.YAxis.Title.Text = "window size";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].plist, GraphColors[k%6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);

            // Offset Y space between point and label
            // NOTE:  This offset is in Y scale units, so it depends on your actual data
            //const double offset = 1.0;
            #region add text labels
            // Loop to add text labels to the points
            /*for (int i = 0; i < count; i++)
            {
                // Get the pointpair
                PointPair pt = curve.Points[i];

                // Create a text label from the Y data value
                TextObj text = new TextObj(pt.Y.ToString("f2"), pt.X, pt.Y + offset,
                    CoordType.AxisXYScale, AlignH.Left, AlignV.Center);
                text.ZOrder = ZOrder.A_InFront;
                // Hide the border and the fill
                text.FontSpec.Border.IsVisible = false;
                text.FontSpec.Fill.IsVisible = false;
                //text.FontSpec.Fill = new Fill( Color.FromArgb( 100, Color.White ) );
                // Rotate the text to 90 degrees
                text.FontSpec.Angle = 90;

                myPane.GraphObjList.Add(text);
            }*/
            #endregion

            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
        public void CreateFlowRateChart(ZedGraphControl zgc, Flow[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, s";
            myPane.YAxis.Title.Text = "Receive rate (Mbps)";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].flow_rate_list, GraphColors[k % 6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);

            // Offset Y space between point and label
            // NOTE:  This offset is in Y scale units, so it depends on your actual data
            //const double offset = 1.0;
            #region add text labels
            // Loop to add text labels to the points
            /*for (int i = 0; i < count; i++)
            {
                // Get the pointpair
                PointPair pt = curve.Points[i];

                // Create a text label from the Y data value
                TextObj text = new TextObj(pt.Y.ToString("f2"), pt.X, pt.Y + offset,
                    CoordType.AxisXYScale, AlignH.Left, AlignV.Center);
                text.ZOrder = ZOrder.A_InFront;
                // Hide the border and the fill
                text.FontSpec.Border.IsVisible = false;
                text.FontSpec.Fill.IsVisible = false;
                //text.FontSpec.Fill = new Fill( Color.FromArgb( 100, Color.White ) );
                // Rotate the text to 90 degrees
                text.FontSpec.Angle = 90;

                myPane.GraphObjList.Add(text);
            }*/
            #endregion

            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
        public void CreatePacketDelayChart(ZedGraphControl zgc, Flow[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, s";
            myPane.YAxis.Title.Text = "Packet delay (s)";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].packet_delay_list, GraphColors[k % 6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);
            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
        public void CreateSendRateChart(ZedGraphControl zgc, Flow[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, s";
            myPane.YAxis.Title.Text = "Send rate (Mbps)";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].send_rate_list, GraphColors[k % 6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);

            // Offset Y space between point and label
            // NOTE:  This offset is in Y scale units, so it depends on your actual data
            //const double offset = 1.0;

            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
        public void CreateDroppedPacketChart(ZedGraphControl zgc, Link[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, ms";
            myPane.YAxis.Title.Text = "Packet loss (packets/s)";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].droppedPacketList, GraphColors[k % 6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);

            // Offset Y space between point and label
            // NOTE:  This offset is in Y scale units, so it depends on your actual data
            //const double offset = 1.0;
            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
        public void CreateBufferSizeChart(ZedGraphControl zgc, Link[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, ms";
            myPane.YAxis.Title.Text = "Buffer size (packets)";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].buffer_size_list, GraphColors[k % 6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);

            // Offset Y space between point and label
            // NOTE:  This offset is in Y scale units, so it depends on your actual data
            //const double offset = 1.0;
            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
        public void CreateLinkRateChart(ZedGraphControl zgc, Link[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, s";
            myPane.YAxis.Title.Text = "Link rate (Mbps)";

            /*myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95, 0.15, CoordType.PaneFraction,
                                 AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10;*/
            myPane.Legend.Position = LegendPos.InsideTopLeft;
            // Add a curve
            for (int k = 0; k < list.Length; k++)
            {
                LineItem curve = myPane.AddCurve(list[k].name, list[k].link_rate_list, GraphColors[k % 6], SymbolType.None);
                curve.Line.Width = 2.0F;
                curve.Line.IsAntiAlias = true;
                curve.Symbol.Fill = new Fill(Color.White);
                curve.Symbol.Size = 7;
            }
            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, Color.ForestGreen), 45.0F);

            // Offset Y space between point and label
            // NOTE:  This offset is in Y scale units, so it depends on your actual data
            //const double offset = 1.0;
            // Leave some extra space on top for the labels to fit within the chart rect
            myPane.YAxis.Scale.MaxGrace = 0.2;

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();
        }
    }
}
