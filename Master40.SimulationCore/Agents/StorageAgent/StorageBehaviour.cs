using Akka.Actor;
using AkkaSim.Interfaces;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    public static class StorageBehaviour
    {
        public const string StockElement = "StockElement";
        public const string RequestedItems = "RequestedItems";
        public const string StockFor = "StockFor";
        public const string ProviderList = "ProviderList";

        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>
        public static BehaviourSet Default(Stock stockElement)
        {
            var actions = new Dictionary<Type, Action<Agent, ISimulationMessage>>();
            var properties = new Dictionary<string, object>();

            actions.Add(typeof(StorageAgent.Instruction.RequestArticle), RequestArticle);
            actions.Add(typeof(StorageAgent.Instruction.StockRefill), StockRefill);
            actions.Add(typeof(StorageAgent.Instruction.ResponseFromProduction), ResponseFromProduction);
            
            
            stockElement.StockExchanges.Add(new StockExchange
            {
                StockId = stockElement.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = stockElement.StartValue,
                State = State.Finished,
                RequiredOnTime = 0,
                Time = 0
            });

            properties.Add(StockElement, stockElement);
            properties.Add(RequestedItems, new List<RequestItem>());
            properties.Add(ProviderList, new List<IActorRef>());
            properties.Add(StockFor, stockElement.Article.Name);

            return new BehaviourSet(actions);
        }

        public static Action<Agent, ISimulationMessage> RequestArticle = (agent, item) =>
        {

            // debug
            agent.DebugMessage(" requests Article " + agent.Get<Stock>(StockElement).Name + " from Stock Agent ->" + agent.Sender.Path.Name);

            // cast Request
            if (!(item.Message is RequestItem requestItem))
                throw new InvalidCastException("Cast to Request Item Failed");

            // try to make Reservation
            var stockReservation = StorageAgent.MakeReservationFor(agent, requestItem);
            if (!stockReservation.IsInStock)
            {
                // add to Request queue if not in Stock
                agent.Get<List<RequestItem>>(RequestedItems).Add(requestItem);
            }
            // Create Callback // Probably not required here
            agent.Send(DispoAgent.Instruction.ResponseFromStock.Create(stockReservation, agent.Sender));
        };

        public static Action<Agent, ISimulationMessage> StockRefill = (agent, item) =>
        {
            var exchangeId = (Guid)item.Message;
            var stockElement = agent.Get<Stock>(StockElement);
            // TODO: Retrun Request Itme with id of Stock Exchange
            var stockExchange = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);
            
            // stock Income 
            agent.DebugMessage(" income " + stockElement.Article.Name + " quantity " + stockExchange.Quantity + " added to Stock");
            stockElement.Current += stockExchange.Quantity;
            // change element State to Finish
            stockExchange.State = State.Finished;
            //stockExchange.RequiredOnTime = (int)Context.TimePeriod;
            stockExchange.Time = (int)agent.CurrentTime;

            var requestedItems = agent.Get<List<RequestItem>>(RequestedItems);
            // no Items to be served.
            if (!RequestedItems.Any()) return;
            
            // Try server all Nonserved Items.
            foreach (var request in requestedItems.OrderBy(x => x.DueTime).ToList()) // .Where( x => x.DueTime <= Context.TimePeriod))
            {
                var notServed = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == request.StockExchangeId);
                if (notServed != null) { notServed.State = State.Finished; notServed.Time = (int)agent.CurrentTime; }
                else throw new Exception("No StockExchange found");
                agent.Send(DispoAgent.Instruction.RequestProvided.Create(request, request.Requester));
                requestedItems.Remove(request);
            }
        };

        //private void ResponseFromProduction(RequestItem item)
        public static Action<Agent, ISimulationMessage> ResponseFromProduction = (agent, item) =>
        {
            var requestItem = item.Message as RequestItem;
            var stockElement = agent.Get<Stock>(StockElement);
            var providerList = agent.Get<List<IActorRef>>(ProviderList);
            var requestedItems = agent.Get<List<RequestItem>>(RequestedItems);

            if (requestItem == null)
            {
                throw new InvalidCastException(agent.Name + " failed to Cast ProductionAgent on Instruction.ObjectToProcess");
            }

            agent.DebugMessage("Production Agent Finished Work: " + agent.Sender.Path.Name);


            // Add the Produced item to Stock
            stockElement.Current++;
            var stockExchange = new StockExchange
            {
                StockId = stockElement.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = 1,
                State = State.Finished,
                RequiredOnTime = (int)agent.CurrentTime,
                Time = (int)agent.CurrentTime
            };
            stockElement.StockExchanges.Add(stockExchange);

            providerList.Add(agent.Sender);
            // Check if the most Important Request can be provided.
            var requestProvidable = requestedItems.FirstOrDefault(x => x.DueTime == requestedItems.Min(r => r.DueTime));
            // TODO: Prove if quantity check is required.

            if (requestProvidable.IsHeadDemand && requestProvidable.DueTime > agent.CurrentTime) { return; }
            // else
            StorageAgent.ProvideArticle(agent, requestProvidable, requestedItems);
        };
    }
}
