using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using static Master40.SimulationCore.Agents.Hub.Instruction;
using static Master40.SimulationCore.Agents.Hub.Properties;

namespace Master40.SimulationCore.Agents
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time,string skillGroup, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Hub(actorPaths, time, skillGroup, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, string skillGroup, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            var skg = this.Get<string>(SKILL_GROUP);
            skg = skillGroup;
        }
    }
}
