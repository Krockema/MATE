using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.DirectoryAgent.Types;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using static FAgentInformations;
using static FBreakDowns;
using static FCapabilityProviderDefinitions;
using static FResourceHubInformations;
using static FResourceInformations;
using static FResourceTypes;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                       : base(childMaker: null, simulationType: simulationType) { }


        internal HubManager hubManager { get; set; } = new HubManager();
        internal HubManager storageManager { get; set; } = new HubManager();
 
        public override bool Action(object message)
        {
            switch (message)
            {
                case Directory.Instruction.Default.CreateStorageAgents msg: CreateStorageAgents(stock: msg.GetObjectFromMessage); break;
                case Directory.Instruction.Default.CreateMachineAgents msg: CreateMachineAgents(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Default.RequestAgent msg: RequestAgent(discriminator: msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBrakeDown(breakDown: msg.GetObjectFromMessage); break;
                case Directory.Instruction.Default.ForwardRegistrationToHub msg: ForwardRegistrationToHub(msg.GetObjectFromMessage); break;
                case Directory.Instruction.Default.CreateResourceHubAgents msg: CreateResourceHubAgents(capabilityDefinition: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void CreateResourceHubAgents(FResourceHubInformation capabilityDefinition)
        {
            // Create Capability based Hub Agent for each Capability of resource
            var capability = capabilityDefinition.Capability as M_ResourceCapability;
            var hub = hubManager.GetHubActorRefBy(capability.Name);
            if (hub == null)
            {
                var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                        , time: Agent.CurrentTime
                        , simtype: SimulationType
                        , maxBucketSize: capabilityDefinition.MaxBucketSize
                        , workTimeGenerator: capabilityDefinition.WorkTimeGenerator as WorkTimeGenerator
                        , debug: Agent.DebugThis
                        , principal: Agent.Context.Self)
                    , name: ("Hub(" + capability.Name + ")").ToActorName());

                System.Diagnostics.Debug.WriteLine($"Created Hub {capability.Name} with {capabilityDefinition.MaxBucketSize} !");
                hubManager.AddOrCreateRelation(hubAgent, capability);
            }
        }

        private void ForwardRegistrationToHub(FResourceInformation resourceInformation)
        {
            var capabilites = resourceInformation.ResourceCapabilityProvider.Select(x => x.ResourceCapability.ParentResourceCapability).Distinct();
            foreach (M_ResourceCapability capability in capabilites)
            {
                var hub = hubManager.GetHubActorRefBy(capability.Name);
                // it is probably neccesary to do this for each sub capability.
                var filtered = resourceInformation.ResourceCapabilityProvider.Where(x => x.ResourceCapability.ParentResourceCapabilityId == capability.Id).ToList();
                var resourceInfo = new FResourceInformation(  resourceId: resourceInformation.ResourceId
                                                            , resourceName: resourceInformation.ResourceName
                                                            , resourceCapabilityProvider: filtered
                                                            , requiredFor: capability.Name
                                                            , this.Agent.Context.Sender);
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


        public void CreateMachineAgents(FCapabilityProviderDefinition resourceCapabilityProviderDefinition)
        {
            var resourceCapabilityProvider = resourceCapabilityProviderDefinition.CapabilityProvider as List<M_ResourceCapabilityProvider>;
            var resource = resourceCapabilityProviderDefinition.Resource as M_Resource;
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(props: ResourceAgent.Resource.Props(actorPaths: Agent.ActorPaths
                                                                    , resource: resource
                                                                    , time: Agent.CurrentTime
                                                                    , debug: Agent.DebugThis
                                                                    , principal: Agent.Context.Self)
                                                    , name: ("Resource(" + resource.Name + ")").ToActorName());

            Agent.Send(instruction: BasicInstruction.Initialize
                                                    .Create(target: resourceAgent
                                                         , message: ResourceAgent.Behaviour
                                                                                .Factory.Get(simType: SimulationType
                                                                                 , workTimeGenerator: resourceCapabilityProviderDefinition.WorkTimeGenerator as WorkTimeGenerator
                                                                                 , resourceCapabilityProvider
                                                                                 , timeConstraintQueueLength: resourceCapabilityProviderDefinition.TimeConstraintQueueLength
                                                                                 , resourceId: resource.Id)));
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
