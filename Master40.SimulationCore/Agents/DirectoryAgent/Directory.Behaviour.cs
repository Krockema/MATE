using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Master40.Tools.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Master40.SimulationCore.Agents.Directory.Instruction;
using static Master40.SimulationCore.Agents.Directory.Properties;

namespace Master40.SimulationCore.Agents
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
                { RESSOURCE, new List<RequestRessource>() }
            };
            return new DirectoryBehaviour(properties);
        }

        public void CreateStorageAgents(Directory agent, Stock stock)
        {
            var storage = agent.Context.ActorOf(Storage.Props(agent.ActorPaths
                                            , agent.CurrentTime
                                            , agent.DebugThis
                                            , agent.Context.Self)
                                            , ("Storage(" + stock.Name + ")").ToActorName());

            var ressourceCollection = agent.Get<List<RequestRessource>>(RESSOURCE);
            ressourceCollection.Add(new RequestRessource(stock.Article.Name, ResourceType.Storage, storage));
            
            agent.Send(BasicInstruction.Initialize.Create(storage, StorageBehaviour.Get(stock)));
        }


        public void CreateMachineAgents(Directory agent, RessourceDefinition ressource)
        {
            var machine = ressource.Resource as Machine;
            var machineAgent = agent.Context.ActorOf(Resource.Props(agent.ActorPaths
                                            , machine
                                            , ressource.WorkTimeGenerator as WorkTimeGenerator
                                            , agent.CurrentTime
                                            , agent.DebugThis
                                            , agent.Context.Self)
                                            , ("Machine(" + machine.Name + ")").ToActorName());

            var ressourceCollection = agent.Get<List<RequestRessource>>(RESSOURCE);
            ressourceCollection.Add(new RequestRessource(machine.Name, ResourceType.Machine, machineAgent));

            agent.Send(BasicInstruction.Initialize.Create(machineAgent, ResourceBehaviour.Get()));
        }



        // public static Action<Agent, ISimulationMessage> CreateHubAgent = (agent, item) =>
        // {
        //     var hubInfo = item.Message as RessourceDefinition;
        //     var requiredFor = ((Machine)hubInfo.Resource).MachineGroup.Name;
        //     // Create ComunicationAgent if not existent
        //     var hubAgent = agent.Context.ActorOf(props: HubAgent.Props(actorPaths: agent.ActorPaths
        //                                                             , time: agent.CurrentTime
        //                                                             , skillGroup: requiredFor
        //                                                             , debug: agent.DebugThis)
        //                                                             , name: "Hub(" + requiredFor + ")");
        // 
        //     var ressourceCollection = agent.Get<List<RequestRessource>>(Ressource);
        //     ressourceCollection.Add(new RequestRessource(requiredFor, ResourceType.Production, hubAgent));
        // 
        //     // agent.Send(BasicInstruction.Initialize.Create(HubBehaviour.Default(), hubAgent));
        //     agent.Send(HubAgent.Instruction.AddMachineToHub.Create(new HubInformation(ResourceType.Machine, requiredFor, agent.Sender), hubAgent));
        // };

        private void RequestRessourceAgent(Directory agent, string descriminator)
        {
            // debug
            agent.DebugMessage(" got Called for Storage by -> " + agent.Sender.Path.Name);

            // find the related Comunication Agent
            var ressourceCollection = agent.Get<List<RequestRessource>>(RESSOURCE);
            var ressource = ressourceCollection.First(x => x.Discriminator == descriminator);

            var hubInfo = new HubInformation(ressource.ResourceType
                                                , descriminator
                                                , ressource.actorRef);

            // Tell the Requester the corrosponding Comunication Agent.
            agent.Send(BasicInstruction.ResponseFromHub
                                       .Create(message: hubInfo, target: agent.Sender));
        }
    }
}
