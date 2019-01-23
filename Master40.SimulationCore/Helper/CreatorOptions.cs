using Akka.Actor;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Helper
{
    public static class CreatorOptions
    {
        public static Func<IUntypedActorContext, AgentSetup, IActorRef> ContractCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(Contract.Props(setup.ActorPaths, setup.Time, setup.Debug));
        };

        public static Func<IUntypedActorContext, AgentSetup, IActorRef> DispoCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(Dispo.Props(setup.ActorPaths, setup.Time, setup.Debug, setup.Principal));
        };
    }
}
