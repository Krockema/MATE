using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Helper;
using static FHubInformations;
using static FResourceTypes;

namespace Master40.SimulationCore.Agents.ResourceAgent
{
    // Alt: CapacityAgent, ProducerAgent, PotencialFactorAgent, SchedulingAgent, 
    public partial class Resource : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, M_Resource resource, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Resource(actorPaths, resource, workTimeGenerator, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, M_Resource resource, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            //this.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(ResourceType.Machine, machine.MachineGroup.Name, this.Self), principal));
            this.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(FResourceType.Machine, this.Name, this.Self), principal));
        }
    }
}