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
    public partial class Example5 : Form
    {
        Simulation sim;
        public DataSet ds;

        double interPingTime;

        int FishingID = 9000;
        int CargoID = 8000;
        int NavyID = 1000;
        int AircraftID = 7000;

        public Example5()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ds = SharpSim.DataSetProcessor.RunRead();
            sim = new Simulation(true, 1, false);

            sim.CreateEvents(ds.Tables["Event"]);

            Run(sim.events["1"]);
            AircraftArrival(sim.events["A2"]);
            StartPatrol(sim.events["A3"]);
            EndPatrol(sim.events["A4"]);
            CheckEnvironment(sim.events["A5"]);
            ReportPirate(sim.events["A6"]);
            Terminate(sim.events["100"]);

            sim.CreateEdges(ds.Tables["Edge"]);
            sim.CreateStats(ds.Tables["Stat"]);
            //sim.Run();
            sim.StartSimulationThread();
        }
        public void Run(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                
            };
        }
        public void Ping(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
               
            };
        }
        public void AircraftArrival(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
                
            };
        }
        public void StartPatrol(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
            };
        }
        public void EndPatrol(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
            };
        }
        public void CheckEnvironment(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
            };
        }
        public void ReportPirate(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
            };
        }


        public void Temp(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {
            };
        }
        public void Terminate(Event evt)
        {
            evt.EventExecuted += delegate(object obj1, EventInfoArgs e)
            {

            };
        }



    }
}
