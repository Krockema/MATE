using System;
using Akka.Actor;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Types
{
    public interface IBehaviour
    {
        bool Action(object message);
        Agent Agent { get; set; }
        Func<IUntypedActorContext, AgentSetup, IActorRef> ChildMaker { get; }
        SimulationType SimulationType { get; }
        bool AfterInit();
        bool PostAdvance();
        void OnChildAdd(IActorRef actorRef);
    }
}
