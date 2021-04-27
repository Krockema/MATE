using System;
using Akka.Actor;
using Mate.Production.Core.Agents.ContractAgent;
using Mate.Production.Core.Agents.DispoAgent;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.Guardian.Options
{
    public static class ChildMaker
    {

        private static int contractCount = 0;

        private static int productionCount = 0;

        private static int dispoCount = 0;

        public static Func<IUntypedActorContext, AgentSetup, IActorRef> ContractCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(props: Contract.Props(actorPaths: setup.ActorPaths, configuration: setup.Configuration, time: setup.Time, debug: setup.Debug), name: "ContractAgent(" + contractCount++ + ")");
        };

        public static Func<IUntypedActorContext, AgentSetup, IActorRef> DispoCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(props: Dispo.Props(actorPaths: setup.ActorPaths, configuration: setup.Configuration, time: setup.Time, debug: setup.Debug, principal: setup.Principal), name: "DispoAgent(" + productionCount++ + ")");
        };

        public static Func<IUntypedActorContext, AgentSetup, IActorRef> ProductionCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(props: ProductionAgent.Production.Props(actorPaths: setup.ActorPaths, configuration: setup.Configuration, time: setup.Time, debug: setup.Debug, principal: setup.Principal), name: "ProductionAgent(" + dispoCount++ + ")");
        };
    }
}
