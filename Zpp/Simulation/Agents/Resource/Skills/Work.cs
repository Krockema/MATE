using Akka.Actor;
using AkkaSim.Definitions;
using Zpp.Common.ProviderDomain.Wrappers;

namespace Zpp.Simulation.Agents.Resource.Skills
{
    public class Work : SimulationMessage
    {
        public static Work Create(ProductionOrderOperation operation, IActorRef target)
        {
            return new Work(operation, target);
        }
        private Work(object message, IActorRef target) : base(message, target)
        { }
        public ProductionOrderOperation GetOperation => this.Message as ProductionOrderOperation;
    }
}
