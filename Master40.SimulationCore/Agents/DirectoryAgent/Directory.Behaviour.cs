using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.Ressource;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.DirectoryAgent
{
    public class DirectoryBehaviour : Behaviour
    {
        private DirectoryBehaviour(Dictionary<string, object> properties) : base(null, properties) { }

        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>
        public static DirectoryBehaviour Get()
        {
            var properties = new Dictionary<string, object>
            {
                { Directory.Properties.RESSOURCE, new List<FRequestRessource>() },
                
            };
            return new DirectoryBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Directory.Instruction.CreateStorageAgents msg: CreateStorageAgents((Directory)agent, msg.GetObjectFromMessage); break;
                case Directory.Instruction.CreateMachineAgents msg: CreateMachineAgents((Directory)agent, msg.GetObjectFromMessage); break;
                case Directory.Instruction.RequestRessourceAgent msg: RequestRessourceAgent((Directory)agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ResourceBrakeDown msg: ResourceBrakeDown((Directory)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void ResourceBrakeDown(Directory agent, FBreakDown breakDown)
        {
            var hubAgents = agent.Get<List<FRequestRessource>>(Directory.Properties.RESSOURCE);
            var hub = hubAgents.Single(x => x.Discriminator == breakDown.MachineGroup && x.ResourceType == ResourceType.Hub);
            agent.Send(BasicInstruction.ResourceBrakeDown.Create(breakDown, hub.actorRef));
            System.Diagnostics.Debug.WriteLine("Break for " + breakDown.Machine, "Directory");
        }

        public void CreateStorageAgents(Directory agent, M_Stock stock)
        {
            var storage = agent.Context.ActorOf(Storage.Props(agent.ActorPaths
                                            , agent.CurrentTime
                                            , agent.DebugThis
                                            , agent.Context.Self)
                                            , ("Storage(" + stock.Name + ")").ToActorName());

            var ressourceCollection = agent.Get<List<FRequestRessource>>(Directory.Properties.RESSOURCE);
            ressourceCollection.Add(new FRequestRessource(stock.Article.Name, ResourceType.Storage, storage));
            
            agent.Send(BasicInstruction.Initialize.Create(storage, StorageBehaviour.Get(stock)));
        }


        public void CreateMachineAgents(Directory agent, FResourceSetupDefinition resourceSetupDefinition)
        {
            var resourceSetups = resourceSetupDefinition.ResourceSetup as List<M_ResourceSetup>;


            var hubAgents = agent.Get<List<FRequestRessource>>(Directory.Properties.RESSOURCE);

            // Create Skill based Hub Agent for each Skill of resource
            foreach (var resourceSetup in resourceSetups)
            {
                var hub = hubAgents.FirstOrDefault(x => x.Discriminator == resourceSetup.ResourceSkill.Name && x.ResourceType == ResourceType.Hub);
                if (hub == null)
                {
                    var hubAgent = agent.Context.ActorOf(Hub.Props(agent.ActorPaths
                                                                 , agent.CurrentTime
                                                                 , resourceSetup.ResourceSkill.Name
                                                                 , agent.DebugThis
                                                                 , agent.Context.Self)
                                                        , "Hub(" + resourceSetup.ResourceSkill.Name + ")");
                    //agent.Send(BasicInstruction.Initialize.Create(agent.Context.Self, HubBehaviour.Get(machine.MachineGroup.Name)));
                    hub = new FRequestRessource(resourceSetup.ResourceSkill.Name, ResourceType.Hub, hubAgent);
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
                                                                                                 && x.ResourceType == ResourceType.Hub).actorRef)
                                                    , ("Machine(" + resource.Name + ")").ToActorName());
            agent.Send(BasicInstruction.Initialize.Create(resourceAgent, ResourceBehaviour.Get()));

        }

        private void RequestRessourceAgent(Directory agent, string descriminator)
        {
            // debug
            agent.DebugMessage(" got Called for Storage by -> " + agent.Sender.Path.Name);

            // find the related Comunication Agent
            var ressourceCollection = agent.Get<List<FRequestRessource>>(Directory.Properties.RESSOURCE);
            var ressource = ressourceCollection.First(x => x.Discriminator == descriminator);

            var hubInfo = new FHubInformation(ressource.ResourceType
                                                , descriminator
                                                , ressource.actorRef);

            // Tell the Requester the corrosponding Comunication Agent.
            agent.Send(BasicInstruction.ResponseFromHub
                                       .Create(message: hubInfo, target: agent.Sender));
        }
    }
}
