using Akka.Actor;
using AkkaSim.Definitions;
using Zpp.Mrp.MachineManagement;

namespace Master40.SimulationMrp.Simulation.Agents.JobDistributor.Skills
{
    public class AddResources : SimulationMessage
    {
        public static AddResources Create(ResourceDictionary machines, IActorRef target)
        {
            return new AddResources(machines, target);
        }
        private AddResources(object message, IActorRef target) : base(message, target)
        {
        }
        public ResourceDictionary GetMachines => this.Message as ResourceDictionary;
    }
}
