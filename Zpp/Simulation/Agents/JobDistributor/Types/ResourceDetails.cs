using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using System.Collections.Generic;
using System.Linq;
using Zpp.Mrp.MachineManagement;

namespace Zpp.Simulation.Agents.JobDistributor.Types
{
    public class ResourceDetails
    {
        public Mrp.MachineManagement.Resource Resource  { get; set; }
        public bool IsWorking { get; set; }
        public IActorRef ResourceRef { get; set; }

        public ResourceDetails(Mrp.MachineManagement.Resource resource, IActorRef resourceRef )
        {
            Resource = resource;
            ResourceRef = resourceRef;
        }

    }
}
