using Akka.Actor;
using System;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.Guardian.Options
{
    public static class CreateFunctionProvider
    {
        [ThreadStatic]
        private static int contractCount = 0;
        [ThreadStatic]
        private static int productionCount = 0;
        [ThreadStatic]
        private static int dispoCount = 0;

        public static Func<IUntypedActorContext, AgentSetup, IActorRef> ContractCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(Contract.Props(setup.ActorPaths, setup.Time, setup.Debug), "ContractAgent(" + contractCount++ + ")");
        };

        public static Func<IUntypedActorContext, AgentSetup, IActorRef> DispoCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(Dispo.Props(setup.ActorPaths, setup.Time, setup.Debug, setup.Principal), "DispoAgent(" + productionCount++ + ")");
        };


        public static Func<IUntypedActorContext, AgentSetup, IActorRef> ProductionCreator = (ctx, setup) =>
        {
            return ctx.ActorOf(Production.Props(setup.ActorPaths, setup.Time, setup.Debug, setup.Principal), "ProductionAgent(" + dispoCount++ + ")");
        };
    }
}
