using Akka.Actor;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
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
    }
}
