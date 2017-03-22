using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NExcel;
using SharpSim;

namespace SharpSimExamples
{
    public partial class Example2 : Form
    {
        Simulation sim;
       
        Event eRun; 
        Event eArrival;
        Event eStart;
        Event eEndService;
        Event eTerminate;
        
        Edge edge1_2;
        Edge edge2_2;
        Edge edge2_3;
        Edge edge3_4;
        Edge edge4_3;

        int ID;
        private Button button1;
        private RichTextBox richTextBox1;
        private OpenFileDialog openFileDialog1; // identification number of the customer
        int S; //number of available servers

        public Example2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sim = new Simulation(true, 10, false);

            eRun = new Event("1", "Run", 1, 0);
            eArrival = new Event("2", "Arrival", 4);
            eStart = new Event("3", "Start", 2);
            eEndService = new Event("4", "EndService", 3);
            eTerminate = new Event("5", "Terminate", 5, 50);
             

            //State change listener
            // First do state changes and later event schedules (creating edge)
            Run(eRun);
            Arrival(eArrival);
            Start(eStart);
            EndService(eEndService);
            Terminate(eTerminate);

            //Create edge (event schedule)
            edge1_2 = new Edge("1-2", eRun, eArrival);
            edge2_2 = new Edge("2-2", eArrival, eArrival);
            edge2_2.distEnum = RandomGenerate.dist.exponential;
            //edge2_2.distEnum = RandomGenerate.dist.exponential;
            edge2_2.param1 = 5.0;
            //edge2_2.dist = "exponential";
            //edge2_2.param1 = 5.0;
            edge2_3 = new Edge("2-3", eArrival, eStart);
            edge3_4 = new Edge("3-4", eStart, eEndService);
            edge3_4.distEnum = RandomGenerate.dist.exponential;
            edge3_4.param1 = 5.0;
            //edge3_4.dist = "exponential";
            //edge3_4.param1 = 5.0;
            edge4_3 = new Edge("4-3", eEndService, eStart);
                     
            //extra ability
            //sim.events.Add(eRun.no, eRun);
            //sim.edges.Add(edge1_2.name, edge1_2);

            sim.CreateStats("2-4");
            
            sim.Run();

        }

        public void Run(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                ID = 1;
                S = 2;

                Customer customer = new Customer(ID);
                edge1_2.attribute = customer;
            };
        }

        public void Arrival(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                //parameter passing icin bir exception. Once Event'in icindeki kuyruga parameter
                //degeri sokulur. Sonra bu kuyrugun basindaki parametre degeri (Customer) alinir.

                eStart.queue.Add(e.evnt.parameter);
                edge2_3.attribute = eStart.queue[0];

                ID++;
                Customer cust = new Customer(ID);
                edge2_2.attribute = cust;

                if (S > 0)
                    edge2_3.condition = true;
                else
                    edge2_3.condition = false;
            };
        }
        
        public void Start(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                eStart.queue.RemoveAt(0);                 
                S--;
                edge3_4.attribute = e.evnt.parameter;
            };
        }

        public void EndService(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                S++;
                if (eStart.queue.Count() == 0)
                    edge4_3.condition = false;
                else
                {
                    edge4_3.condition = true;
                    edge4_3.attribute = eStart.queue[0];
                }

                Stats.CollectStats("2-4", e.evnt.parameter.ReturnInterval("2", "4"));
            };
        }

        public void Terminate(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                richTextBox1.Text += "Replication No : " + Simulation.replicationNow + " ended." + "\n";
                Stats.AddDataToStatsGlobalDictionary("2-4", Stats.Dictionary["2-4"].mean);                
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
            this.button1.Location = new System.Drawing.Point(191, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(76, 42);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(17, 70);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(250, 139);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Example2
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Example2";
            this.ResumeLayout(false);

        }

       
    }

    class Customer : Entity
    {
        
        public Customer(int id)
            : base (id)
        {
            this.identifier = id;
        }
    }
}
