using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Helper;
using static FAgentInformations;
using static FResourceTypes;

namespace Master40.SimulationCore.Agents.ResourceAgent
{
    // Alt: CapacityAgent, ProducerAgent, PotencialFactorAgent, SchedulingAgent, 
    public partial class Resource : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, M_Resource resource, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, resource, workTimeGenerator, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, M_Resource resource, WorkTimeGenerator workTimeGenerator, long time, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            //this.Send(Hub.Instruction.AddMachineToHub.Create(new FHubInformation(ResourceType.Machine, machine.MachineGroup.Name, this.Self), principal));
            this.Send(instruction: Hub.Instruction.AddMachineToHub.Create(message: new FAgentInformation(fromType: FResourceType.Machine, requiredFor: this.Name, @ref: this.Self), target: principal));
        }
    }
}