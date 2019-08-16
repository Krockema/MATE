using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Helper;
using System.Collections.Generic;
using System.Linq;
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
                       : base(null, simulationType) { }


        internal List<FRequestResource> fRequestResources { get; set; }
        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>


        public override bool Action(object message)
        {
            switch (message)
            {
                case Directory.Instruction.CreateStorageAgents msg: CreateStorageAgents(msg.GetObjectFromMessage); break;
                case Directory.Instruction.CreateMachineAgents msg: CreateMachineAgents(msg.GetObjectFromMessage); break;
                case Directory.Instruction.RequestAgent msg: RequestAgent(msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBrakeDown(msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBrakeDown(FBreakDown breakDown)
        {
            var hub = fRequestResources.Single(x => x.Discriminator == breakDown.ResourceSkill && x.ResourceType == FResourceType.Hub);
            Agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, hub.actorRef));
            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Resource, "Directory");
        }

        public void CreateStorageAgents(M_Stock stock)
        {
            var storage = Agent.Context.ActorOf(Storage.Props(Agent.ActorPaths
                                            , Agent.CurrentTime
                                            , Agent.DebugThis
                                            , Agent.Context.Self)
                                            , ("Storage(" + stock.Name + ")").ToActorName());

            var ressourceCollection = fRequestResources;
            ressourceCollection.Add(new FRequestResource(stock.Article.Name, FResourceType.Storage, storage));

            Agent.Send(BasicInstruction.Initialize.Create(storage, StorageAgent.Behaviour.Factory.Get(stock, SimulationType)));
        }


        public void CreateMachineAgents(FResourceSetupDefinition resourceSetupDefinition)
        {
            var resourceSetups = resourceSetupDefinition.ResourceSetup as List<M_ResourceSetup>;


            var hubAgents = fRequestResources;

            // Create Skill based Hub Agent for each Skill of resource
            foreach (var resourceSetup in resourceSetups)
            {
                var hub = hubAgents.FirstOrDefault(x => x.Discriminator == resourceSetup.ResourceSkill.Name && x.ResourceType == FResourceType.Hub);
                if (hub == null)
                {
                    var hubAgent = Agent.Context.ActorOf(Hub.Props(Agent.ActorPaths
                                                                 , Agent.CurrentTime
                                                                 , SimulationType
                                                                 , Agent.DebugThis
                                                                 , Agent.Context.Self)
                                                        , "Hub(" + resourceSetup.ResourceSkill.Name + ")");
                    //Agent.Send(BasicInstruction.Initialize.Create(Agent.Context.Self, HubBehaviour.Get(machine.MachineGroup.Name)));
                    hub = new FRequestResource(resourceSetup.ResourceSkill.Name, FResourceType.Hub, hubAgent);
                    hubAgents.Add(hub);
                }
            }

            var resource = resourceSetups.FirstOrDefault().Resource as M_Resource;
            // Create resource If Required
            var resourceAgent = Agent.Context.ActorOf(Resource.Props(Agent.ActorPaths
                                                                    , resource
                                                                    , resourceSetupDefinition.WorkTimeGenerator as WorkTimeGenerator
                                                                    , Agent.CurrentTime
                                                                    , Agent.DebugThis
                                                                    // TODO : 1 Machine N Hubs.
                                                                    , hubAgents.FirstOrDefault(x => x.Discriminator == resource.ResourceSetups.First().ResourceSkill.Name
                                                                                                 && x.ResourceType == FResourceType.Hub).actorRef)
                                                    , ("Machine(" + resource.Name + ")").ToActorName());
            Agent.Send(BasicInstruction.Initialize.Create(resourceAgent, ResourceAgent.Behaviour.Factory.Get(SimulationType)));

        }

        private void RequestAgent(string discriminator)
        {
            Agent.DebugMessage($" got called for Agent by:  {Agent.Sender.Path.Name} for: {discriminator}");

            // find the related Hub/Storage Agent
            var agentToProvide = fRequestResources.First(x => x.Discriminator == discriminator);

            var hubInfo = new FAgentInformation(agentToProvide.ResourceType
                                                , discriminator
                                                , agentToProvide.actorRef);

            // Tell the Requester the corresponding Agent
            Agent.Send(BasicInstruction.ResponseFromDirectory
                                       .Create(message: hubInfo, target: Agent.Sender));
        }

    }
}
