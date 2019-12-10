using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using AkkaSim;
using Master40.SimulationMrp.Simulation.Agents.Resource.Skills;

namespace Master40.SimulationMrp.Simulation.Monitors
{
    public class WorkTimeMonitor : SimulationMonitor
    {
        public WorkTimeMonitor(long time) 
            : base(time, new List<Type> { typeof(FinishWork) })
        {
        }

        protected override void EventHandle(object o)
        {
            // base.EventHandle(o);
            var m = o as FinishWork;
            var material = m.GetOperation;
            Debug.WriteLine("Finished: " + material.GetValue().Name + " on Time: " + _Time);
        }

        public static Props Props(long time)
        {
            return Akka.Actor.Props.Create(() => new WorkTimeMonitor(time));
        }
    }
}
