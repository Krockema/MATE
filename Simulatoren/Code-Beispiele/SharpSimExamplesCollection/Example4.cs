using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using NExcel;
using SharpSim;

///This is a modified version of Example1. Only difference is read simulation info from xml document.
namespace SharpSimExamples
{
    public partial class Example4 : Form
    {
        Simulation sim;
        int ID;
        private Button button1;
        private RichTextBox richTextBox1;
        private OpenFileDialog openFileDialog1;
        int S;
        public DataSet ds;

        public Example4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ds = DataSetProcessor.RunRead();

            sim = new Simulation(true, 3, true);

            sim.CreateEvents(ds.Tables["Event"]);

            Run(sim.events["1"]);
            Arrival(sim.events["2"]);
            Start(sim.events["3"]);
            EndService(sim.events["4"]);
            Terminate(sim.events["5"]);

            sim.CreateEdges(ds.Tables["Edge"]);
            sim.CreateStats(ds.Tables["Stat"]);
            //sim.Run();
            sim.StartSimulationThread();
        }

        

        /// <summary>
        /// An event handler for Run event
        /// </summary>
        /// <param name="evt">Event to run</param>
        public void Run(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                ID = 1;
                S = 1;

                Customer customer = new Customer(ID);
                sim.edges["1-2"].attribute = customer;

            };
        }
        public void Arrival(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                sim.events["3"].queue.Add(e.evnt.parameter);
                sim.edges["2-3"].attribute = sim.events["3"].queue[0];

                ID++;
                Customer cust = new Customer(ID);
                sim.edges["2-2"].attribute = cust;

                if (S > 0)
                    sim.edges["2-3"].condition = true;
                else
                    sim.edges["2-3"].condition = false;
                
            };
        }

        public void Start(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                sim.events["3"].queue.RemoveAt(0);
                S--;
                sim.edges["3-4"].attribute = e.evnt.parameter;

            };
        }

        public void EndService(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                S++;
                if (sim.events["3"].queue.Count() > 0)
                {
                    sim.edges["4-3"].attribute = sim.events["3"].queue[0];
                }
                if (sim.events["3"].queue.Count() == 0)
                    sim.edges["4-3"].condition = false;
                else
                    sim.edges["4-3"].condition = true;
                Stats.CollectStats("2-4", e.evnt.parameter.ReturnInterval("2", "4"));
            };

        }

        public void Terminate(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                Stats.AddDataToStatsGlobalDictionary("2-4", Stats.Dictionary["2-4"].mean);
            };
        }
        /// <summary>
        /// An Entity Class
        /// </summary>
        class Customer : Entity
        {

            public Customer(int id)
                : base(id)
            {
                this.identifier = id;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

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
            this.button1.Location = new System.Drawing.Point(22, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 38);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(22, 73);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(204, 88);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Example1
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Example1";
            this.ResumeLayout(false);

        }
    }
}
