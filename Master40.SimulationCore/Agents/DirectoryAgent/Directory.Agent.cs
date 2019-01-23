using Akka.Actor;
using System.Collections.Generic;
using Master40.SimulationImmutables;
using System;
using Master40.SimulationCore.Helper;
using Master40.DB.Models;
using Master40.Tools.Simulation;
using Master40.SimulationCore.Agents;

namespace Master40.SimulationCore.Agents
{
    public partial class Directory : Agent
    {

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new Directory(actorPaths, time, debug));
        }

        private Directory(ActorPaths actorPaths, long time, bool debug) : base(actorPaths, time, debug, ActorRefs.Nobody)
        {

        }        
    }
}
