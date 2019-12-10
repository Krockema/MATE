using Akka.Actor;

namespace Master40.SimulationMrp.Simulation.Agents.JobDistributor.Types
{
    public class ResourceDetails
    {
        public Zpp.Mrp.MachineManagement.Resource Resource { get; set; }
        public bool IsWorking { get; set; }
        public IActorRef ResourceRef { get; set; }

        public ResourceDetails(Zpp.Mrp.MachineManagement.Resource resource, IActorRef resourceRef)
        {
            Resource = resource;
            ResourceRef = resourceRef;
        }

    }
}