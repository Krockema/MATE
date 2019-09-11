using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Helper;
using System.Collections.Generic;
using System.Linq;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using static FBreakDowns;
using static FAgentInformations;
using static FRequestResources;
using static FResourceSetupDefinitions;
using static FResourceTypes;

namespace Master40.SimulationCore.Agents.DirectoryAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                       : base(childMaker: null, simulationType: simulationType) { }


        internal List<FRequestResource> fRequestResources { get; set; } = new List<FRequestResource>();
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
                default: return false;
            }
            return true;
        }

        private void ResourceBrakeDown(FBreakDown breakDown)
        {
            var hub = fRequestResources.Single(predicate: x => x.Discriminator == breakDown.ResourceSkill && x.ResourceType == FResourceType.Hub);
            Agent.Send(instruction: BasicInstruction.ResourceBrakeDown.Create(message: breakDown, target: hub.actorRef));
            System.Diagnostics.Debug.WriteLine(message: "Break for " + breakDown.Resource, category: "Directory");
        }

        public void CreateStorageAgents(M_Stock stock)
        {
            var storage = Agent.Context.ActorOf(props: Storage.Props(actorPaths: Agent.ActorPaths
                                            , time: Agent.CurrentTime
                                            , debug: Agent.DebugThis
                                            , principal: Agent.Context.Self)
                                            , name: ("Storage(" + stock.Name + ")").ToActorName());

            var resourceCollection = fRequestResources;
            resourceCollection.Add(item: new FRequestResource(discriminator: stock.Article.Name, resourceType: FResourceType.Storage, actorRef: storage));

            Agent.Send(instruction: BasicInstruction.Initialize.Create(target: storage, message: StorageAgent.Behaviour.Factory.Get(stockElement: stock, simType: SimulationType)));
        }


        public void CreateMachineAgents(FResourceSetupDefinition resourceSetupDefinition)
        {
            var resourceSetups = resourceSetupDefinition.ResourceSetup as List<M_ResourceSetup>;

            // Create Skill based Hub Agent for each Skill of resource
            // TODO Currently Resource can only have one Skill and discriminator between subskills are made by resourceTool
            foreach (var resourceSetup in resourceSetups)
            {
                var hub = fRequestResources.FirstOrDefault(predicate: x => x.Discriminator == resourceSetup.ResourceSkill.Name && x.ResourceType == FResourceType.Hub);
                if (hub == null)
                {
                    var hubAgent = Agent.Context.ActorOf(props: Hub.Props(actorPaths: Agent.ActorPaths
                                                                 , time: Agent.CurrentTime
                                                                 , simtype: SimulationType
                                                                 , debug: Agent.DebugThis
                                                                 , principal: Agent.Context.Self)
                                                        , name: "Hub(" + resourceSetup.ResourceSkill.Name + ")");
                    //Agent.Send(BasicInstruction.Initialize.Create(Agent.Context.Self, HubBehaviour.Get(machine.MachineGroup.Name)));
                    hub = new FRequestResource(discriminator: resourceSetup.ResourceSkill.Name, resourceType: FResourceType.Hub, actorRef: hubAgent);
                    fRequestResources.Add(item: hub);
                }
            }

            var resource = resourceSetups.FirstOrDefault().Resource as M_Resource;
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(props: Resource.Props(actorPaths: Agent.ActorPaths
                                                                    , resource: resource
                                                                    , time: Agent.CurrentTime
                                                                    , debug: Agent.DebugThis
                                                                    // TODO : Handle 1 resource in multiply hub agents
                                                                    , principal: fRequestResources.FirstOrDefault(predicate: x => x.Discriminator == resource.ResourceSetups.First().ResourceSkill.Name
                                                                                                 && x.ResourceType == FResourceType.Hub).actorRef)
                                                    , name: ("Machine(" + resource.Name + ")").ToActorName());


            Agent.Send(instruction: BasicInstruction.Initialize
                                                    .Create(target: resourceAgent
                                                         , message: ResourceAgent.Behaviour
                                                                                .Factory.Get(simType: SimulationType
                                                                                 , workTimeGenerator: resourceSetupDefinition.WorkTimeGenerator as WorkTimeGenerator 
                                                                                 , resourceId: resource.Id
                                                                                 , toolManager: new ToolManager(resourceSetups: resourceSetups))));
        }

        private void RequestAgent(string discriminator)
        {
            Agent.DebugMessage(msg: $" got called for Agent by:  {Agent.Sender.Path.Name} for: {discriminator}");

            // find the related Hub/Storage Agent
            var agentToProvide = fRequestResources.First(predicate: x => x.Discriminator == discriminator);

            var hubInfo = new FAgentInformation(fromType: agentToProvide.ResourceType
                                                , requiredFor: discriminator
                                                , @ref: agentToProvide.actorRef);

            // Tell the Requester the corresponding Agent
            Agent.Send(instruction: BasicInstruction.ResponseFromDirectory
                                       .Create(message: hubInfo, target: Agent.Sender));
        }

    }
}
