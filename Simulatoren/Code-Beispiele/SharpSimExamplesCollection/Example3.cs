using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpSim;

namespace SharpSimExamples
{
    public partial class Example3 : Form
    {
        Simulation sim;

        Event eRun;
        Event eArrival;
        Event eStartProduction;
        Event eEndProduction;
        Event eRepair;
        Event eFailure;
        Event eTerminate;

        Edge edge1_2;
        Edge edge2_2;
        Edge edge2_3;
        Edge edge3_4;
        Edge edge4_3;
        Edge edge1_6;
        Edge edge5_3;
        Edge edge5_6;
        Edge edge6_5;
        Edge cedge6_4;


        int ID; // identification number of the customer
        int S; //number of available servers
        int UnsatisfiedCustomer; //Unsatisfied Customer 
        int LostDemandSize; //Lost Demand Size
        int Demand;//Demand
        int OnHand;//On-Hand Inventory
        int BatchSize;
        int r;//re-order Point
        int bigR;
        private Button button1;
        private RichTextBox richTextBox1;
        private OpenFileDialog openFileDialog1;//up-to inventory level
        double unsatisfiedRatio;

        public Example3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sim = new Simulation(false, 10, true);

            eRun = new Event("1", "Run", 1, 0);
            eArrival = new Event("2", "Arrival", 6);
            eStartProduction = new Event("3", "StartProduction", 4);
            eEndProduction = new Event("4", "EndProduction", 5);
            eRepair = new Event("5", "Repair", 2);
            eFailure = new Event("6", "Failure", 3);
            eTerminate = new Event("7", "Terminate", 7, 1000000);


            //State change listener
            // First do state changes and later event schedules (creating edge)
            Run(eRun);
            Arrival(eArrival);
            StartProduction(eStartProduction);
            EndProduction(eEndProduction);
            Repair(eRepair);
            Failure(eFailure);
            Terminate(eTerminate);

            //Create edge (event schedule)
            edge1_2 = new Edge("1-2", eRun, eArrival);

            //Version 1.1 property
            List<object> edge1_2Dist = new List<object>();
            edge1_2Dist.Add("uniform");
            edge1_2Dist.Add(180.0);
            edge1_2Dist.Add(420.0);
            edge1_2.distribution = edge1_2Dist;

            //Version 1.0 edge type
            //edge1_2.dist = "uniform";
            //edge1_2.param1 = 180.0;
            //edge1_2.param2 = 420.0;

            edge2_2 = new Edge("2-2", eArrival, eArrival);
            edge2_2.dist = "uniform";
            edge2_2.param1 = 180.0;
            edge2_2.param2 = 420.0;
            edge2_3 = new Edge("2-3", eArrival, eStartProduction);
            edge3_4 = new Edge("3-4", eStartProduction, eEndProduction);
            edge3_4.dist = "uniform";
            edge3_4.param1 = 10;
            edge3_4.param2 = 20;
            edge4_3 = new Edge("4-3", eEndProduction, eStartProduction);
            edge1_6 = new Edge("1-6", eRun, eFailure);
            edge1_6.dist = "exponential";
            edge1_6.param1 = 200.0;
            edge5_3 = new Edge("5-3", eRepair, eStartProduction);
            edge5_6 = new Edge("5-6", eRepair, eFailure);
            edge5_6.dist = "exponential";
            edge5_6.param1 = 200.0;
            edge6_5 = new Edge("6-5", eFailure, eRepair);
            edge6_5.dist = "normal";
            edge6_5.param1 = 90.0;
            edge6_5.param2 = 45.0;
            //Version 1.1 property 
            //Cancelling edge last parameter of edge indicates cancelling edge
            cedge6_4 = new Edge("6-4", eFailure, eEndProduction, true);

            sim.CreateStats("SOHtimeAverage");
            sim.CreateStats("Unsatisfied Customer");
            sim.CreateStats("Lost Demand Size");
            sim.CreateStats("Total Customer");
            sim.CreateStats("Unsatisfied Ratio");
            sim.Run();
        }

        public void Run(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                ID = 0;
                S = 1;
                UnsatisfiedCustomer = 0;
                LostDemandSize = 0;
                OnHand = 250;
                Stats.CollectStats("SOHtimeAverage", new double[2]{ Simulation.clock, OnHand });
                BatchSize = 5;
                r = 150;
                bigR = 500;
                
            };
        }
        public void Arrival(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                ID++;
                Demand = RandomGenerate.rnd.Next(50, 100);
                
                if (OnHand > Demand)
                {
                    OnHand -= Demand;
                    Stats.CollectStats("SOHtimeAverage", new double[2] { Simulation.clock, OnHand });
                }
                else
                {
                    LostDemandSize += Demand - OnHand;
                    Stats.CollectStats("Lost Demand Size", LostDemandSize);
                    OnHand = 0;
                    Stats.CollectStats("SOHtimeAverage", new double[2] { Simulation.clock, OnHand });
                    UnsatisfiedCustomer++;
                }
                if ((OnHand<r) && (S > 0))
                    edge2_3.condition = true;
                else
                    edge2_3.condition = false;
            };
        }

        public void StartProduction(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                S--;
            };
        }

        public void EndProduction(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                S++;
                OnHand += BatchSize;
                Stats.CollectStats("SOHtimeAverage", new double[2] { Simulation.clock, OnHand });


                if (OnHand < bigR)
                    edge4_3.condition = true;
                else
                    edge4_3.condition = false;
            };
        }

        public void Repair(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                S++;
                if (OnHand < r)
                {
                    edge5_3.condition = true;
                }
                else
                {
                    edge5_3.condition = false;
                }
                edge5_6.condition = true;
            };
        }

        public void Failure(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                S--;
                cedge6_4.cancellingEdge = true;
                edge6_5.condition = true;
            };
        }

        public void Terminate(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                unsatisfiedRatio = (double) UnsatisfiedCustomer / ID;
                richTextBox1.Text += "Replication No : " + Simulation.replicationNow + " ended." + "\n";
                Stats.AddDataToStatsGlobalDictionary("SOHtimeAverage", Stats.Dictionary["SOHtimeAverage"].timeWeightedAverage);
                Stats.AddDataToStatsGlobalDictionary("Unsatisfied Customer", UnsatisfiedCustomer);
                Stats.AddDataToStatsGlobalDictionary("Total Customer", ID);
                Stats.AddDataToStatsGlobalDictionary("Lost Demand Size", Stats.Dictionary["Lost Demand Size"].mean);
                Stats.AddDataToStatsGlobalDictionary("Unsatisfied Ratio", unsatisfiedRatio);
            };
        }

        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(167, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 52);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(11, 80);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(256, 160);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Example3
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Example3";
            this.ResumeLayout(false);

        }
    }    
}
