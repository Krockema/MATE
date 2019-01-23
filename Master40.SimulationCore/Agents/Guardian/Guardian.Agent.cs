using Akka.Actor;
using AkkaSim;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    /// <summary>
    /// Guardian Action is an Supervising Actor to Create Child Actors on Command and Controll their LifeCycle
    /// </summary>
    public partial class Guardian : Agent
    {
        /// <summary>
        /// Basic Agent
        /// </summary>
        /// <param name="actorPaths"></param>
        /// <param name="time">Current time span</param>
        /// <param name="debug">Parameter to activate Debug Messages on Agent level</param>
        private Guardian(ActorPaths actorPaths, long time, bool debug)
            : base(actorPaths, time, false, null)
        {
            DebugMessage("I'm alive: " + Self.Path.ToStringWithAddress());
        }

        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new Guardian(actorPaths, time, debug));
        }
    }
}
