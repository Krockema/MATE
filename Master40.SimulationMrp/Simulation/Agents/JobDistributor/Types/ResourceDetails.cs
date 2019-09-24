using Akka.Actor;
using Zpp.Mrp.MachineManagement;

namespace Master40.SimulationMrp.Simulation.Agents.JobDistributor.Types
{
    public class ResourceDetails
    {
        public Machine Machine  { get; set; }
        public bool IsWorking { get; set; }
        public IActorRef ResourceRef { get; set; }

        public ResourceDetails(Machine machine, IActorRef resourceRef )
        {
            Machine = machine;
            ResourceRef = resourceRef;
        }

    }
}
