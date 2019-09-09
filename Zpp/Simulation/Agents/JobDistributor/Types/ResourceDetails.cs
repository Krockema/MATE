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
