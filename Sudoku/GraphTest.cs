using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class GraphTest : Form
    {
        private System.ComponentModel.IContainer components = null;
        System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        Queue<int> q;
        public Object objlock;

        public GraphTest( Queue<int> qu,  Object l  )
        {
            objlock = l;
            q = qu;
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            update(q);
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            this.chart1.Size = new System.Drawing.Size(284, 262);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);

            // 
            // GraphTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.chart1);
            this.Name = "GraphTest";
            this.Text = "Graph";
            this.Load += new System.EventHandler(this.GraphTest_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);

        }
        private void GraphTest_Load(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = (1000);
            timer.Tick += new EventHandler(timer1_Tick);
            timer.Start();
            update(q);
        }
        public void update(Queue<int> qt)
        {
            chart1.Series.Clear();
            var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series1",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line
            };

            this.chart1.Series.Add(series1);

            lock(objlock)
            {
                for (int i = 0; i < qt.Count; i++)
                {
                    series1.Points.AddXY(i, qt.ElementAt(i));
                    i++;
                }
            }
            chart1.Invalidate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            update(q);
        }
    }
}
