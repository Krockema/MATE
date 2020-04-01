using System;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using Master40.SimulationCore.Agents.DirectoryAgent.Types;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;
using static FBreakDowns;
using static FAgentInformations;
using static FRequestResources;
using static FResourceSetupDefinitions;
using static FResourceTypes;
using static FResourceInformations;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                       : base(childMaker: null, simulationType: simulationType) { }


        internal HubManager hubManager { get; set; } = new HubManager();
        internal HubManager storageManager { get; set; } = new HubManager();
        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>


        public override bool Action(object message)
        {
            switch (message)
            {
                case Directory.Instruction.CreateStorageAgents msg: CreateStorageAgents(stock: msg.GetObjectFromMessage); break;
                case Directory.Instruction.CreateMachineAgents msg: CreateMachineAgents(resourceSetupDefinition: msg.GetObjectFromMessage); break;
                case Directory.Instruction.RequestAgent msg: RequestAgent(discriminator: msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBrakeDown(breakDown: msg.GetObjectFromMessage); break;
                case Directory.Instruction.ForwardRegistrationToHub msg: ForwardRegistrationToHub(setupList: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ForwardRegistrationToHub(List<M_ResourceSetup> setupList)
        {
            var capabilites = setupList.Select(x => x.ResourceCapability).Distinct();

            List<IActorRef> hubRefs = new List<IActorRef>();
            foreach(M_ResourceCapability capability in capabilites)
            {
                var hub = hubManager.GetHubActorRefBy(capability.Name);
                // it is probably neccesary to do this for each sub capability.
                var resourceInfo = new FResourceInformation(new List<M_ResourceCapability> { capability }
                                                            , capability.Name
                                                            , this.Agent.Context.Self);
                Agent.Send(Hub.Instruction.Default.AddResourceToHub.Create(resourceInfo, hub));
                
            }
        }

        private void ResourceBrakeDown(FBreakDown breakDown)
        {
            var hub = hubManager.GetHubActorRefBy(breakDown.ResourceCapability);
            Agent.Send(instruction: BasicInstruction.ResourceBrakeDown.Create(message: breakDown, target: hub));
            System.Diagnostics.Debug.WriteLine(message: "Break for " + breakDown.Resource, category: "Directory");
        }

        public void CreateStorageAgents(M_Stock stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , time: Agent.CurrentTime
                                            , debug: Agent.DebugThis
                                            , principal: Agent.Context.Self)
                                            , name: ("Storage(" + stock.Name + ")").ToActorName());

            storageManager.AddOrCreateRelation(storage, stock.Article.Name);
            Agent.Send(instruction: BasicInstruction.Initialize.Create(target: storage, message: StorageAgent.Behaviour.Factory.Get(stockElement: stock, simType: SimulationType)));
        }


        public void CreateMachineAgents(FResourceSetupDefinition resourceSetupDefinition)
        {
            var resourceSetups = resourceSetupDefinition.ResourceSetup as List<M_ResourceSetup>;

            // Create Capability based Hub Agent for each Capability of resource
            // TODO Currently Resource can only have one Capability and discriminator between subCapabilities are made by resourceTool
            foreach (var resourceSetup in resourceSetups)
            {
                var hub = hubManager.GetHubActorRefBy(resourceSetup.ResourceCapability.Name);
                if (hub == null)
                {
                    var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                                                                 , time: Agent.CurrentTime
                                                                 , simtype: SimulationType
                                                                 , maxBucketSize: resourceSetupDefinition.MaxBucketSize
                                                                 , debug: Agent.DebugThis
                                                                 , principal: Agent.Context.Self)
                                                        , name: "Hub(" + resourceSetup.ResourceCapability.Name + ")");

                    System.Diagnostics.Debug.WriteLine($"Created Hub {resourceSetup.ResourceCapability.Name} with {resourceSetupDefinition.MaxBucketSize} !");

                    hubManager.AddOrCreateRelation(hubAgent, resourceSetup.ResourceCapability);
                }
            }

            var resource = resourceSetups.FirstOrDefault().ChildResource as M_Resource;
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(props: Resource.Props(actorPaths: Agent.ActorPaths
                                                                    , resource: resource
                                                                    , time: Agent.CurrentTime
                                                                    , debug: Agent.DebugThis
                                                                    , principal: Agent.Context.Self)
                                                    , name: ("Resource(" + resource.Name + ")").ToActorName());

            resourceSetups.Where(x => x.ChildResource.Id == resource.Id)
                          .ForEach(x => x.ChildResource.IResourceRef = resourceAgent);
            Agent.Send(instruction: BasicInstruction.Initialize
                                                    .Create(target: resourceAgent
                                                         , message: ResourceAgent.Behaviour
                                                                                .Factory.Get(simType: SimulationType
                                                                                 , workTimeGenerator: resourceSetupDefinition.WorkTimeGenerator as WorkTimeGenerator
                                                                                 , toolManager: new ToolManager(resourceSetups: resourceSetups))));
        }

        /// <summary>
        /// TODO: ResourceType usage is not the best solution, and Resource Type might not be required.
        /// </summary>
        /// <param name="discriminator"></param>
        private void RequestAgent(string discriminator)
        {
            FResourceType type = FResourceType.Hub;
            // find the related Hub/Storage Agent
            var agentToProvide = hubManager.GetHubActorRefBy(discriminator);
            if (agentToProvide == null) { 
                type = FResourceType.Storage;
                agentToProvide = storageManager.GetHubActorRefBy(discriminator);
            } 

            if(agentToProvide == null) throw new Exception("no Resource found!");

            var hubInfo = new FAgentInformation(fromType: type
                , requiredFor: discriminator
                , @ref: agentToProvide);
            // Tell the Requester the corresponding Agent
            Agent.Send(instruction: BasicInstruction.ResponseFromDirectory
                                       .Create(message: hubInfo, target: Agent.Sender));
        }

    }
}
