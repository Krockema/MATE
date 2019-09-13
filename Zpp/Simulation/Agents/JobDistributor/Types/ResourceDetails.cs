using Akka.Actor;

namespace Zpp.Simulation.Agents.JobDistributor.Types
{
    public class ResourceDetails
    {
        public Mrp.MachineManagement.Resource Resource { get; set; }
        public bool IsWorking { get; set; }
        public IActorRef ResourceRef { get; set; }

        public ResourceDetails(Mrp.MachineManagement.Resource resource, IActorRef resourceRef)
        {
            Resource = resource;
            ResourceRef = resourceRef;
        }

    }
}