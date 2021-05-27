using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.DirectoryAgent.Types;
using Mate.Production.Core.Agents.HubAgent;
using Mate.Production.Core.Agents.StorageAgent;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using static FAgentInformations;
using static FBreakDowns;
using static FCapabilityProviderDefinitions;
using static FResourceHubInformations;
using static FResourceInformations;
using static FResourceTypes;

namespace Mate.Production.Core.Agents.DirectoryAgent.Behaviour
{
    public class Queuing : Core.Types.Behaviour
    {
        internal Queuing(SimulationType simulationType)
                       : base(childMaker: null, simulationType: simulationType) { }


        internal IActorRef _hubAgentActorRef { get; set; } = ActorRefs.Nobody;
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
                case Directory.Instruction.Default.CreateResourceHubAgents msg: CreateResourceHubAgents(msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void CreateResourceHubAgents(FResourceHubInformation resourceHubInformation)
        {
            if (!_hubAgentActorRef.Equals(ActorRefs.Nobody))
                return;

            var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                    , configuration: Agent.Configuration
                    , time: Agent.CurrentTime
                    , simtype: SimulationType
                    , maxBucketSize: 0 // not used currently
                    , workTimeGenerator: resourceHubInformation.WorkTimeGenerator as WorkTimeGenerator
                    , debug: Agent.DebugThis
                    , principal: Agent.Context.Self)
                , name: "CentralHub");

            _hubAgentActorRef = hubAgent;

            System.Diagnostics.Debug.WriteLine($"Created Central Hub !");
        }

        private void ForwardRegistrationToHub(FResourceInformation resourceInformation)
        {
            var capabilites = resourceInformation.ResourceCapabilityProvider.Select(x => x.ResourceCapability.ParentResourceCapability).Distinct();
            foreach (M_ResourceCapability capability in capabilites)
            {
                var hub = _hubAgentActorRef;
                // it is probably neccesary to do this for each sub capability.
                var filtered = resourceInformation.ResourceCapabilityProvider.Where(x => x.ResourceCapability.ParentResourceCapabilityId == capability.Id).ToList();
                var resourceInfo = new FResourceInformation(resourceId: resourceInformation.ResourceId
                                                            , resourceName: resourceInformation.ResourceName
                                                            , resourceCapabilityProvider : filtered
                                                            , resourceType: resourceInformation.ResourceType
                                                            , requiredFor: capability.Name
                                                            , this.Agent.Context.Sender);
                Agent.Send(Hub.Instruction.Default.AddResourceToHub.Create(resourceInfo, hub));

            }
        }

        private void ResourceBrakeDown(FBreakDown breakDown)
        {
            var hub = _hubAgentActorRef;
            Agent.Send(instruction: BasicInstruction.ResourceBrakeDown.Create(message: breakDown, target: hub));
            System.Diagnostics.Debug.WriteLine(message: "Break for " + breakDown.Resource, category: "Directory");
        }

        public void CreateStorageAgents(M_Stock stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , configuration: Agent.Configuration
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
                                                                    , configuration: Agent.Configuration
                                                                    , resource: resource
                                                                    , time: Agent.CurrentTime
                                                                    , debug: Agent.DebugThis
                                                                    , measurementActorRef: Agent.ActorPaths.MeasurementAgent.Ref
                                                                    , principal: Agent.Context.Self)
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
            FResourceType type = FResourceType.Storage;
            type = FResourceType.Hub;
            // find the related Hub/Storage Agent
            var agentToProvide = storageManager.GetHubActorRefBy(discriminator);
            if (agentToProvide == null)
            {
                type = FResourceType.Hub;
                agentToProvide = _hubAgentActorRef;
                
            }

            if (agentToProvide == null) throw new Exception("no Resource found!");

            var hubInfo = new FAgentInformation(fromType: type
                , requiredFor: discriminator
                , @ref: agentToProvide);
            // Tell the Requester the corresponding Agent
            Agent.Send(instruction: BasicInstruction.ResponseFromDirectory
                                       .Create(message: hubInfo, target: Agent.Sender));
        }

    }
}
