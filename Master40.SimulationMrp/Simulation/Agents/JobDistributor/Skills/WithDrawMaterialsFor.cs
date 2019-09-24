using Akka.Actor;
using AkkaSim.Definitions;
using Zpp.Common.ProviderDomain.Wrappers;

namespace Master40.SimulationMrp.Simulation.Agents.JobDistributor.Skills
{
    public class WithDrawMaterialsFor : SimulationMessage
    {
        public static WithDrawMaterialsFor Create(ProductionOrderOperation machines, IActorRef target)
        {
            return new WithDrawMaterialsFor(machines, target);
        }
        private WithDrawMaterialsFor(object message, IActorRef target) : base(message, target)
        {
        }
        public ProductionOrderOperation GetOperation => this.Message as ProductionOrderOperation;
    }
}
