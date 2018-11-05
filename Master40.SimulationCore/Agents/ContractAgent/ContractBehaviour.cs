using AkkaSim.Interfaces;
using Master40.DB.Models;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using static Master40.SimulationCore.Agents.ContractAgent.Instruction;

namespace Master40.SimulationCore.Agents
{
    public static class ContractBehaviour
    {
        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>
        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            actions.Add(typeof(StartOrder), StartOrder);
            actions.Add(typeof(Finish), Finish);

            return new BehaviourSet(actions);
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param agent="ContractAgent"></param>
        /// <param startOrder="ISimulationMessage"></param>
        public static Action<Agent, ISimulationMessage> StartOrder = (agent, startOrder) => 
        {
            var orderItem = startOrder.Message as OrderPart;
            // Create Dispo Agent
            var dispo = agent.Context.ActorOf(props: DispoAgent.Props(actorPaths: agent.ActorPaths
                                                                    , time: agent.CurrentTime
                                                                   , debug: agent.DebugThis)
                                      , name: "Dispo(" + orderItem.Id + ")");
            // init
            agent.Send(BasicInstruction.Initialize.Create(DispoBehaviour.Default(), dispo)); 
            
            // create Request Item
            RequestItem requestItem = orderItem.ToRequestItem(requester: dispo);
            // Send Request
            agent.Send(DispoAgent.Instruction.RequestArticle.Create(requestItem, dispo));

            agent.DebugMessage("Dispo<" + requestItem.Article.Name + "(OrderId: " + orderItem.Id + ") >");

            agent.ValueStore.Add(ContractAgent.RequestItem, requestItem);
        };

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="instructionSet"></param>
        private static Action<Agent, ISimulationMessage> Finish = (agent, message) =>
        {
            var item = message.Message as RequestItem;
            agent.DebugMessage("Dispo Said Done.");
            var localItem = agent.Get<RequestItem>(ContractAgent.RequestItem);
            // try to Finish if time has come
            if (agent.CurrentTime >= item.DueTime)
            {
                agent.Send(SystemAgent.Instruction.OrderProvided.Create(item, agent.ActorPaths.SystemAgent.Ref));
                ((ContractAgent)agent).TryFinialize();
            }
        };
    }
}
