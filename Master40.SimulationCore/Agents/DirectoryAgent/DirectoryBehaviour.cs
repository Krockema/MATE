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
using static Master40.SimulationCore.Agents.DirectoryAgent.Instruction;

namespace Master40.SimulationCore.Agents
{
    public static class DirectoryBehaviour
    {
        public const string Ressource = "Ressource";

        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>
        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            actions.Add(typeof(CreateStorageAgents), CreateStorageAgents);
            actions.Add(typeof(CreateMachineAgents), CreateMachineAgents);
            actions.Add(typeof(RequestRessourceAgent), RequestRessourceAgent);

            properties.Add(Ressource, new List<RequestRessource>());

            return new BehaviourSet(actions, properties);
        }

        public static Action<Agent, ISimulationMessage> CreateStorageAgents = (agent, item) =>
        {
            var stock = item.Message as Stock;
            var storage = agent.Context.ActorOf(StorageAgent.Props(agent.ActorPaths
                                            , agent.CurrentTime
                                            , agent.DebugThis)
                                            , ("Storage(" + stock.Name + ")").ToActorName());

            var ressourceCollection = agent.Get<List<RequestRessource>>(Ressource);
            ressourceCollection.Add(new RequestRessource(stock.Article.Name, ResourceType.Storage, storage));
            
            agent.Send(BasicInstruction.Initialize.Create(StorageBehaviour.Default(stock), storage));
        };

        public static Action<Agent, ISimulationMessage> CreateMachineAgents = (agent, item) =>
        {
            var resource = item.Message as RessourceDefinition;
            var machine = resource.Resource as Machine;
            var machineAgent = agent.Context.ActorOf(ResourceAgent.Props(agent.ActorPaths
                                            , machine
                                            , resource.WorkTimeGenerator as WorkTimeGenerator
                                            , agent.CurrentTime
                                            , agent.DebugThis)
                                            , ("Machine(" + machine.Name + ")").ToActorName());

            var ressourceCollection = agent.Get<List<RequestRessource>>(Ressource);
            ressourceCollection.Add(new RequestRessource(machine.Name, ResourceType.Machine, machineAgent));

            agent.Send(BasicInstruction.Initialize.Create(ResourceBehaviour.Default(), machineAgent));
        };



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


        public static Action<Agent, ISimulationMessage> RequestRessourceAgent = (agent, item) =>
        {
            var discriminator = item.Message as string;
            // debug
            agent.DebugMessage(" got Called for Storage by -> " + agent.Sender.Path.Name);

            // find the related Comunication Agent
            var ressourceCollection = agent.Get<List<RequestRessource>>(Ressource);
            var ressource = ressourceCollection.First(x => x.Discriminator == discriminator);

            var hubInfo = new HubInformation(ressource.ResourceType
                                                , discriminator
                                                , ressource.actorRef);

            // Tell the Requester the corrosponding Comunication Agent.
            agent.Send(BasicInstruction.ResponseFromHub
                                       .Create(message: hubInfo, target: agent.Sender));
        };  


    }
}
