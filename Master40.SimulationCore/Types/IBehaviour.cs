using System;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Types
{
    public interface IBehaviour
    {
        bool Action(object message);
        Agent Agent { get; set; }
        Func<IUntypedActorContext, AgentSetup, IActorRef> ChildMaker { get; }
        SimulationType SimulationType { get; }
        bool AfterInit();
    }
}
