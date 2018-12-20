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
    public partial class DirectoryAgent : Agent
    {

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new DirectoryAgent(actorPaths, time, debug));
        }

        public DirectoryAgent(ActorPaths actorPaths, long time, bool debug) : base(actorPaths, time, debug)
        {

        }

        private int MyProperty { get; set; }
    }
}
