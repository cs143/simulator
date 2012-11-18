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
    }
    public struct Link
    {
        public string name;
        public PointPairList droppedPacketList;
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
            Flow[] wSizeList = GetWSizeList(xmlDoc);
            CreateWindowSizeChart(zedGraphControl1,wSizeList);
            Link[] linkStatList = GetLinkStatusList(xmlDoc);
            CreateDroppedPacketChart(zedGraphControl2, linkStatList);
        }
        public Link[] GetLinkStatusList(XDocument xmlDoc)
        {
            var linkList = (from item in xmlDoc.Descendants("LinkStatus")
                            select item.Attribute("link_name").Value
                 ).Distinct();
            Link[] list = new Link[linkList.Count()];
            int count = 0;
            double TotTime = Convert.ToDouble(xmlDoc.Descendants("TotalTime").First().Value);
            double TimeInterval = TotTime / 100;
            foreach (var link in linkList)
            {
                list[count] = new Link();
                list[count].name = link;
                list[count].droppedPacketList = new PointPairList();
                Int64 lastDpCount = 0;
                double rate = 0;
                for (int j = 0; j < 100; j++)
                {
                    double startTime = j * TimeInterval;
                    double endTime = (j + 1) * TimeInterval;
                    var dPackItem = (from item in xmlDoc.Descendants("LinkStatus")
                                     where item.Attribute("link_name").Value == link
                                     && Convert.ToDouble(item.Attribute("time").Value) <= endTime
                                     && Convert.ToDouble(item.Attribute("time").Value) >= startTime
                                     orderby (double)item.Attribute("time")
                                     select new
                                     {
                                         time = item.Attribute("time").Value,
                                         dp_count = item.Attribute("dropped_packets").Value
                                     }).Last();
                    if (dPackItem != null)
                    {
                        Int64 realDpCount = Convert.ToInt64(dPackItem.dp_count);
                        rate = (realDpCount - lastDpCount) / TimeInterval;
                        lastDpCount = realDpCount;
                    }
                    list[count].droppedPacketList.Add((j + 1) * TimeInterval, rate);

                }
                count++;
            }
            return list;
        }
        public Flow[] GetWSizeList(XDocument xmlDoc)
        {
            var flowList = (from item in xmlDoc.Descendants("FlowStatus")
                            select item.Attribute("flow_name").Value
                            ).Distinct();
                        
            Flow[] list = new Flow[flowList.Count()];
            int count = 0;
            foreach (var flow in flowList)
            {
                list[count] = new Flow();
                list[count].name = flow;
                list[count].plist = new PointPairList();
                var wSizeItems = from item in xmlDoc.Descendants("FlowStatus")
                                 where item.Attribute("flow_name").Value == flow
                                 orderby (double)item.Attribute("time")
                                 select new
                                 {
                                     time = item.Attribute("time"),
                                     wsize = item.Attribute("window_size")
                                 };
                foreach (var pt in wSizeItems)
                {
                    double x = (double)pt.time;
                    double y = (double)pt.wsize;
                    list[count].plist.Add(x, y);
                }
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
            myPane.XAxis.Title.Text = "Time, ms";
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
        public void CreateDroppedPacketChart(ZedGraphControl zgc, Link[] list)
        {
            GraphPane myPane = zgc.GraphPane;
            System.Drawing.Color[] GraphColors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown };
            // Set the titles and axis labels
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "Time, ms";
            myPane.YAxis.Title.Text = "window size";

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
    }
}
