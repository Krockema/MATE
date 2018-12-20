using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using static Master40.SimulationCore.Agents.ProductionAgent.Instruction;

namespace Master40.SimulationCore.Agents
{
    public static class ProductionBehaviour
    {
        private const string REQUEST_ITEM = "RequestItem";
        private const string WORK_ITEMS = "WorkItems";
        private const string REQUESTED_ITEMS = "RequestedItems";
        private const string HUB_AGENTS = "HubAgents";
        private const string ELEMENT_STATUS = "ElementStatus";
        private const string NEXT_WORK_ITEM = "NextWorkItem";
        

        public static BehaviourSet Default()
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            properties.Add(REQUEST_ITEM, new object()); // RequestItem
            properties.Add(WORK_ITEMS, new List<WorkItem>());
            properties.Add(REQUESTED_ITEMS, new List<RequestItem>());
            properties.Add(HUB_AGENTS, new Dictionary<IActorRef, string>());
            properties.Add(ELEMENT_STATUS, ElementStatus.Created);
            properties.Add(NEXT_WORK_ITEM, new object());

            actions.Add(typeof(ProvideRequest), ProvideRequest);
            // actions.Add(typeof(RequestArticle), RequestArticle);
            // actions.Add(typeof(ResponseFromStock), ResponseFromStock);

            return new BehaviourSet(actions);
        }


        public static Action<Agent, ISimulationMessage> ProvideRequest = (agent, item) =>
        {
            var itemStatus = item.Message as ItemStatus;
            var requestedItems = agent.Get<List<RequestItem>>(REQUESTED_ITEMS);
            var workItems = agent.Get<List<WorkItem>>(WORK_ITEMS);
            var requestItem = requestedItems.Single(x => x.Key == itemStatus.ItemId);
            var status = agent.Get<ElementStatus>(ELEMENT_STATUS);

            agent.DebugMessage("Item to Remove from requestItems: " + requestItem.Article.Name + " --> left " + (workItems.Count() - 1));

            requestedItems.Remove(requestItem);
            if (workItems.Any() && requestedItems.Count() == 0)
            {
                ((ProductionAgent)agent).SetWorkItemReady();
            }
            if (status == ElementStatus.Finished)
            {
                agent.TryToFinish();
            }
        };
    }
}
