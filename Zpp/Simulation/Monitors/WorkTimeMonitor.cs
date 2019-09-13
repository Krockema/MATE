using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using AkkaSim;
using Zpp.Simulation.Agents.Resource;
using Zpp.Simulation.Agents.Resource.Skills;

namespace Zpp.Simulation.Monitors
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
