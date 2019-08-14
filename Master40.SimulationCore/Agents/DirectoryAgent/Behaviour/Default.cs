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


        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Directory.Instruction.CreateStorageAgents msg: CreateStorageAgents(agent, msg.GetObjectFromMessage); break;
                case Directory.Instruction.CreateMachineAgents msg: CreateMachineAgents(agent, msg.GetObjectFromMessage); break;
                case Directory.Instruction.RequestAgent msg: RequestAgent(agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBrakeDown(agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBrakeDown(Agent agent, FBreakDown breakDown)
        {
            var hub = fRequestResources.Single(x => x.Discriminator == breakDown.ResourceSkill && x.ResourceType == FResourceType.Hub);
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, hub.actorRef));
            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Resource, "Directory");
        }

        public void CreateStorageAgents(Agent agent, M_Stock stock)
        {
            var storage = agent.Context.ActorOf(Storage.Props(agent.ActorPaths
                                            , agent.CurrentTime
                                            , agent.DebugThis
                                            , agent.Context.Self)
                                            , ("Storage(" + stock.Name + ")").ToActorName());

            var ressourceCollection = fRequestResources;
            ressourceCollection.Add(new FRequestResource(stock.Article.Name, FResourceType.Storage, storage));

            agent.Send(BasicInstruction.Initialize.Create(storage, StorageAgent.Behaviour.Factory.Get(stock, SimulationType)));
        }


        public void CreateMachineAgents(Agent agent, FResourceSetupDefinition resourceSetupDefinition)
        {
            var resourceSetups = resourceSetupDefinition.ResourceSetup as List<M_ResourceSetup>;


            var hubAgents = fRequestResources;

            // Create Skill based Hub Agent for each Skill of resource
            foreach (var resourceSetup in resourceSetups)
            {
                var hub = hubAgents.FirstOrDefault(x => x.Discriminator == resourceSetup.ResourceSkill.Name && x.ResourceType == FResourceType.Hub);
                if (hub == null)
                {
                    var hubAgent = agent.Context.ActorOf(Hub.Props(agent.ActorPaths
                                                                 , agent.CurrentTime
                                                                 , SimulationType
                                                                 , agent.DebugThis
                                                                 , agent.Context.Self)
                                                        , "Hub(" + resourceSetup.ResourceSkill.Name + ")");
                    //agent.Send(BasicInstruction.Initialize.Create(agent.Context.Self, HubBehaviour.Get(machine.MachineGroup.Name)));
                    hub = new FRequestResource(resourceSetup.ResourceSkill.Name, FResourceType.Hub, hubAgent);
                    hubAgents.Add(hub);
                }
            }

            var resource = resourceSetups.FirstOrDefault().Resource as M_Resource;
            // Create resource If Required
            var resourceAgent = agent.Context.ActorOf(Resource.Props(agent.ActorPaths
                                                                    , resource
                                                                    , resourceSetupDefinition.WorkTimeGenerator as WorkTimeGenerator
                                                                    , agent.CurrentTime
                                                                    , agent.DebugThis
                                                                    // TODO : 1 Machine N Hubs.
                                                                    , hubAgents.FirstOrDefault(x => x.Discriminator == resource.ResourceSetups.First().ResourceSkill.Name
                                                                                                 && x.ResourceType == FResourceType.Hub).actorRef)
                                                    , ("Machine(" + resource.Name + ")").ToActorName());
            agent.Send(BasicInstruction.Initialize.Create(resourceAgent, ResourceAgent.Behaviour.Factory.Get(SimulationType)));

        }

        private void RequestAgent(Agent agent, string descriminator)
        {
            // debug
            agent.DebugMessage(" got called for Agent by:  " + agent.Sender.Path.Name + " for: " + descriminator);

            // find the related Comunication Agent
            var agentToProvide = fRequestResources.First(x => x.Discriminator == descriminator);

            var hubInfo = new FAgentInformation(agentToProvide.ResourceType
                                                , descriminator
                                                , agentToProvide.actorRef);

            // Tell the Requester the corrosponding Agent
            agent.Send(BasicInstruction.ResponseFromDirectory
                                       .Create(message: hubInfo, target: agent.Sender));
        }

    }
}
