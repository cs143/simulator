namespace SimGrapher
{
    partial class GraphWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.zedGraphControl1 = new ZedGraph.ZedGraphControl();
            this.zedGraphControl2 = new ZedGraph.ZedGraphControl();
            this.zedGraphControl3 = new ZedGraph.ZedGraphControl();
            this.zedGraphControl4 = new ZedGraph.ZedGraphControl();
            this.zedGraphControl5 = new ZedGraph.ZedGraphControl();
            this.zedGraphControl6 = new ZedGraph.ZedGraphControl();
            this.dataGridFlows = new System.Windows.Forms.DataGridView();
            this.dataGridLinks = new System.Windows.Forms.DataGridView();
            this.LinkName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgLoss = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgOccup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VarOccup = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FlowName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgThroughput = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AvgDelay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFlows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridLinks)).BeginInit();
            this.SuspendLayout();
            // 
            // zedGraphControl1
            // 
            this.zedGraphControl1.AutoSize = true;
            this.zedGraphControl1.Location = new System.Drawing.Point(13, 23);
            this.zedGraphControl1.Name = "zedGraphControl1";
            this.zedGraphControl1.ScrollGrace = 0D;
            this.zedGraphControl1.ScrollMaxX = 0D;
            this.zedGraphControl1.ScrollMaxY = 0D;
            this.zedGraphControl1.ScrollMaxY2 = 0D;
            this.zedGraphControl1.ScrollMinX = 0D;
            this.zedGraphControl1.ScrollMinY = 0D;
            this.zedGraphControl1.ScrollMinY2 = 0D;
            this.zedGraphControl1.Size = new System.Drawing.Size(621, 328);
            this.zedGraphControl1.TabIndex = 0;
            // 
            // zedGraphControl2
            // 
            this.zedGraphControl2.AutoSize = true;
            this.zedGraphControl2.Location = new System.Drawing.Point(677, 23);
            this.zedGraphControl2.Name = "zedGraphControl2";
            this.zedGraphControl2.ScrollGrace = 0D;
            this.zedGraphControl2.ScrollMaxX = 0D;
            this.zedGraphControl2.ScrollMaxY = 0D;
            this.zedGraphControl2.ScrollMaxY2 = 0D;
            this.zedGraphControl2.ScrollMinX = 0D;
            this.zedGraphControl2.ScrollMinY = 0D;
            this.zedGraphControl2.ScrollMinY2 = 0D;
            this.zedGraphControl2.Size = new System.Drawing.Size(621, 328);
            this.zedGraphControl2.TabIndex = 1;
            // 
            // zedGraphControl3
            // 
            this.zedGraphControl3.AutoSize = true;
            this.zedGraphControl3.Location = new System.Drawing.Point(677, 380);
            this.zedGraphControl3.Name = "zedGraphControl3";
            this.zedGraphControl3.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.zedGraphControl3.ScrollGrace = 0D;
            this.zedGraphControl3.ScrollMaxX = 0D;
            this.zedGraphControl3.ScrollMaxY = 0D;
            this.zedGraphControl3.ScrollMaxY2 = 0D;
            this.zedGraphControl3.ScrollMinX = 0D;
            this.zedGraphControl3.ScrollMinY = 0D;
            this.zedGraphControl3.ScrollMinY2 = 0D;
            this.zedGraphControl3.Size = new System.Drawing.Size(621, 328);
            this.zedGraphControl3.TabIndex = 2;
            // 
            // zedGraphControl4
            // 
            this.zedGraphControl4.AutoSize = true;
            this.zedGraphControl4.Location = new System.Drawing.Point(677, 760);
            this.zedGraphControl4.Name = "zedGraphControl4";
            this.zedGraphControl4.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.zedGraphControl4.ScrollGrace = 0D;
            this.zedGraphControl4.ScrollMaxX = 0D;
            this.zedGraphControl4.ScrollMaxY = 0D;
            this.zedGraphControl4.ScrollMaxY2 = 0D;
            this.zedGraphControl4.ScrollMinX = 0D;
            this.zedGraphControl4.ScrollMinY = 0D;
            this.zedGraphControl4.ScrollMinY2 = 0D;
            this.zedGraphControl4.Size = new System.Drawing.Size(621, 328);
            this.zedGraphControl4.TabIndex = 3;
            // 
            // zedGraphControl5
            // 
            this.zedGraphControl5.AutoSize = true;
            this.zedGraphControl5.Location = new System.Drawing.Point(13, 380);
            this.zedGraphControl5.Name = "zedGraphControl5";
            this.zedGraphControl5.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.zedGraphControl5.ScrollGrace = 0D;
            this.zedGraphControl5.ScrollMaxX = 0D;
            this.zedGraphControl5.ScrollMaxY = 0D;
            this.zedGraphControl5.ScrollMaxY2 = 0D;
            this.zedGraphControl5.ScrollMinX = 0D;
            this.zedGraphControl5.ScrollMinY = 0D;
            this.zedGraphControl5.ScrollMinY2 = 0D;
            this.zedGraphControl5.Size = new System.Drawing.Size(621, 328);
            this.zedGraphControl5.TabIndex = 4;
            // 
            // zedGraphControl6
            // 
            this.zedGraphControl6.AutoSize = true;
            this.zedGraphControl6.Location = new System.Drawing.Point(13, 760);
            this.zedGraphControl6.Name = "zedGraphControl6";
            this.zedGraphControl6.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.zedGraphControl6.ScrollGrace = 0D;
            this.zedGraphControl6.ScrollMaxX = 0D;
            this.zedGraphControl6.ScrollMaxY = 0D;
            this.zedGraphControl6.ScrollMaxY2 = 0D;
            this.zedGraphControl6.ScrollMinX = 0D;
            this.zedGraphControl6.ScrollMinY = 0D;
            this.zedGraphControl6.ScrollMinY2 = 0D;
            this.zedGraphControl6.Size = new System.Drawing.Size(621, 328);
            this.zedGraphControl6.TabIndex = 5;
            // 
            // dataGridFlows
            // 
            this.dataGridFlows.AllowUserToAddRows = false;
            this.dataGridFlows.AllowUserToDeleteRows = false;
            this.dataGridFlows.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridFlows.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FlowName,
            this.AvgThroughput,
            this.AvgDelay});
            this.dataGridFlows.Location = new System.Drawing.Point(13, 1115);
            this.dataGridFlows.Name = "dataGridFlows";
            this.dataGridFlows.Size = new System.Drawing.Size(621, 150);
            this.dataGridFlows.TabIndex = 6;
            // 
            // dataGridLinks
            // 
            this.dataGridLinks.AllowUserToAddRows = false;
            this.dataGridLinks.AllowUserToDeleteRows = false;
            this.dataGridLinks.AllowUserToOrderColumns = true;
            this.dataGridLinks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridLinks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LinkName,
            this.AvgLoss,
            this.AvgOccup,
            this.VarOccup});
            this.dataGridLinks.Location = new System.Drawing.Point(677, 1115);
            this.dataGridLinks.Name = "dataGridLinks";
            this.dataGridLinks.Size = new System.Drawing.Size(621, 150);
            this.dataGridLinks.TabIndex = 7;
            // 
            // LinkName
            // 
            this.LinkName.HeaderText = "Link Name";
            this.LinkName.Name = "LinkName";
            // 
            // AvgLoss
            // 
            this.AvgLoss.HeaderText = "Avg Packet Loss";
            this.AvgLoss.Name = "AvgLoss";
            // 
            // AvgOccup
            // 
            this.AvgOccup.HeaderText = "Avg Buffer Occupancy";
            this.AvgOccup.Name = "AvgOccup";
            // 
            // VarOccup
            // 
            this.VarOccup.HeaderText = "Buffer Occupancy Variance";
            this.VarOccup.Name = "VarOccup";
            // 
            // FlowName
            // 
            this.FlowName.HeaderText = "Flow Name";
            this.FlowName.Name = "FlowName";
            // 
            // AvgThroughput
            // 
            this.AvgThroughput.HeaderText = "Avg Throughput";
            this.AvgThroughput.Name = "AvgThroughput";
            // 
            // AvgDelay
            // 
            this.AvgDelay.HeaderText = "Avg Packet Delay";
            this.AvgDelay.Name = "AvgDelay";
            // 
            // GraphWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMargin = new System.Drawing.Size(10, 10);
            this.ClientSize = new System.Drawing.Size(1342, 753);
            this.Controls.Add(this.dataGridLinks);
            this.Controls.Add(this.dataGridFlows);
            this.Controls.Add(this.zedGraphControl6);
            this.Controls.Add(this.zedGraphControl5);
            this.Controls.Add(this.zedGraphControl4);
            this.Controls.Add(this.zedGraphControl3);
            this.Controls.Add(this.zedGraphControl2);
            this.Controls.Add(this.zedGraphControl1);
            this.Name = "GraphWindow";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Graphs";
            this.Load += new System.EventHandler(this.GraphWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFlows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridLinks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZedGraph.ZedGraphControl zedGraphControl1;
        private ZedGraph.ZedGraphControl zedGraphControl2;
        private ZedGraph.ZedGraphControl zedGraphControl3;
        private ZedGraph.ZedGraphControl zedGraphControl4;
        private ZedGraph.ZedGraphControl zedGraphControl5;
        private ZedGraph.ZedGraphControl zedGraphControl6;
        private System.Windows.Forms.DataGridView dataGridFlows;
        private System.Windows.Forms.DataGridView dataGridLinks;
        private System.Windows.Forms.DataGridViewTextBoxColumn FlowName;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgThroughput;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgDelay;
        private System.Windows.Forms.DataGridViewTextBoxColumn LinkName;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgLoss;
        private System.Windows.Forms.DataGridViewTextBoxColumn AvgOccup;
        private System.Windows.Forms.DataGridViewTextBoxColumn VarOccup;
    }
}

