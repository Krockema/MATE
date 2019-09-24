using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationMrp.Simulation.Agents.JobDistributor.Types;

namespace Master40.SimulationMrp.Simulation.Agents.JobDistributor.Skills
{
    public class OperationsToDistribute : SimulationMessage
    {
        public static OperationsToDistribute Create(OperationManager machine, IActorRef target)
        {
            return new OperationsToDistribute(machine, target);
        }
        private OperationsToDistribute(object message, IActorRef target) : base(message, target)
        {
        }
        public OperationManager GetOperations => this.Message as OperationManager;
    }
}
