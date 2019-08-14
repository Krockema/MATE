using System;
using System.Collections.Generic;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Types
{
    public interface IBehaviour
    {
        bool Action(Agent agent, object message);
        IReadOnlyDictionary<string, object> Properties { get; }
        /// <summary>
        /// Consisting of Context and Principal Agent
        /// Returns ChildReference
        /// </summary>
        Func<IUntypedActorContext, AgentSetup, IActorRef> ChildMaker { get; }
        SimulationType SimulationType { get; }
    }
}
