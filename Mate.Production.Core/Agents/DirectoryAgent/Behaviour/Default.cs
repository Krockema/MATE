﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Agents.DirectoryAgent.Types;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Agents.StorageAgent;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;

namespace Mate.Production.Core.Agents.DirectoryAgent.Behaviour
{
    public class Default : Core.Types.Behaviour
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

        private void CreateResourceHubAgents(ResourceHubInformationRecord capabilityDefinition)
        {
            // Create Capability based Hub Agent for each Capability of resource
            var capability = capabilityDefinition.Capability as M_ResourceCapability;
            var hub = hubManager.GetHubActorRefBy(capability.Name);
            if (hub == null)
            {
                var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                        , configuration: Agent.Configuration
                        , hiveConfig: Agent.HiveConfig
                        , time: Agent.Time
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

        private void ForwardRegistrationToHub(ResourceInformationRecord resourceInformation)
        {
            var capabilites = resourceInformation.ResourceCapabilityProvider.Select(x => x.ResourceCapability.ParentResourceCapability).Distinct();
            foreach (M_ResourceCapability capability in capabilites)
            {
                var hub = hubManager.GetHubActorRefBy(capability.Name);
                // it is probably neccesary to do this for each sub capability.
                var filtered = resourceInformation.ResourceCapabilityProvider.Where(x => x.ResourceCapability.ParentResourceCapabilityId == capability.Id).ToImmutableHashSet();
                var resourceInfo = new ResourceInformationRecord(ResourceId: resourceInformation.ResourceId
                                                            , ResourceName: resourceInformation.ResourceName
                                                            , ResourceCapabilityProvider: filtered
                                                            , ResourceType: resourceInformation.ResourceType
                                                            , WorkTimeGenerator: resourceInformation.WorkTimeGenerator
                                                            , RequiredFor: capability.Name
                                                            , Ref: this.Agent.Context.Sender);
                Agent.Send(Hub.Instruction.Default.AddResourceToHub.Create(resourceInfo, hub));

            }
        }

        private void ResourceBrakeDown(BreakDownRecord breakDown)
        {
            var hub = hubManager.GetHubActorRefBy(breakDown.ResourceCapability);
            Agent.Send(instruction: BasicInstruction.ResourceBrakeDown.Create(message: breakDown, target: hub));
            System.Diagnostics.Debug.WriteLine(message: "Break for " + breakDown.Resource, category: "Directory");
        }

        public void CreateStorageAgents(M_Stock stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , configuration: Agent.Configuration
                                            , hiveConfig: Agent.HiveConfig
                                            , time: Agent.Time
                                            , debug: Agent.DebugThis
                                            , principal: Agent.Context.Self)
                                            , name: ("Storage(" + stock.Name + ")").ToActorName());

            storageManager.AddOrCreateRelation(storage, stock.Article.Name);
            Agent.Send(instruction: BasicInstruction.Initialize.Create(target: storage, message: StorageAgent.Behaviour.Factory.Get(stockElement: stock, simType: SimulationType)));
        }


        public void CreateMachineAgents(CapabilityProviderDefinitionRecord resourceCapabilityProviderDefinition)
        {
            var resourceCapabilityProvider = resourceCapabilityProviderDefinition.CapabilityProvider as List<M_ResourceCapabilityProvider>;
            var resource = resourceCapabilityProviderDefinition.Resource as M_Resource;
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(props: ResourceAgent.Resource.Props(actorPaths: Agent.ActorPaths
                                                                    , hiveConfig: Agent.HiveConfig
                                                                    , configuration: Agent.Configuration
                                                                    , resource: resource
                                                                    , time: Agent.Time
                                                                    , debug: Agent.DebugThis
                                                                    , principal: Agent.Context.Self
                                                                    , measurementActorRef: ActorRefs.Nobody)
                                                    , name: ("Resource(" + resource.Name + ")").ToActorName());

            Agent.Send(instruction: BasicInstruction.Initialize
                                                    .Create(target: resourceAgent
                                                         , message: ResourceAgent.Behaviour
                                                                                .Factory.Get(simType: SimulationType
                                                                                 , workTimeGenerator: resourceCapabilityProviderDefinition.WorkTimeGenerator as WorkTimeGenerator
                                                                                 , resourceCapabilityProvider
                                                                                 , timeConstraintQueueLength: resourceCapabilityProviderDefinition.TimeConstraintQueueLength
                                                                                 , resourceId: resource.Id
                                                                                 , resourceType: resourceCapabilityProviderDefinition.ResourceType)));
        }

        /// <summary>
        /// TODO: ResourceType usage is not the best solution, and Resource Type might not be required.
        /// </summary>
        /// <param name="discriminator"></param>
        private void RequestAgent(string discriminator)
        {
            AgentType type = AgentType.Hub;
            // find the related Hub/Storage Agent
            var agentToProvide = hubManager.GetHubActorRefBy(discriminator);
            if (agentToProvide == null) { 
                type = AgentType.Storage;
                agentToProvide = storageManager.GetHubActorRefBy(discriminator);
            } 

            if(agentToProvide == null) throw new Exception("no Resource found!");

            var hubInfo = new AgentInformationRecord(FromType: type
                , RequiredFor: discriminator
                , Ref: agentToProvide);
            // Tell the Requester the corresponding Agent
            Agent.Send(instruction: BasicInstruction.ResponseFromDirectory
                                       .Create(message: hubInfo, target: Agent.Sender));
        }

    }
}
