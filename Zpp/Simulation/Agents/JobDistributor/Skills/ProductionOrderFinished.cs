using Akka.Actor;
using AkkaSim.Definitions;
using Zpp.Common.ProviderDomain.Wrappers;

namespace Zpp.Simulation.Agents.JobDistributor.Skills
{
    public class ProductionOrderFinished : SimulationMessage
    {
        public static ProductionOrderFinished Create(ProductionOrderOperation operation, IActorRef target)
        {
            return new ProductionOrderFinished(operation, target);
        }
        public ProductionOrderFinished(object message, IActorRef target) : base(message, target)
        {
        }
        public ProductionOrderOperation GetOperation => this.Message as ProductionOrderOperation;
    }
}
