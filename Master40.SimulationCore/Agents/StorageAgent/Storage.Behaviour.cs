using Akka.Actor;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    public class StorageBehaviour : Behaviour
    {
        private StorageBehaviour(Dictionary<string, object> properties) : base(null, properties) { }
        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>
        public static StorageBehaviour Get(Stock stockElement)
        {
            var properties = new Dictionary<string, object>();
                

            stockElement.StockExchanges = new List<StockExchange> {
                                           // Initial Value 
                                                new StockExchange
                                                {
                                                    StockId = stockElement.Id,
                                                    ExchangeType = ExchangeType.Insert,
                                                    Quantity = stockElement.StartValue,
                                                    State = State.Finished,
                                                    RequiredOnTime = 0,
                                                    Time = 0
                                                }};

            properties.Add(Storage.Properties.STOCK_ELEMENT, stockElement);
            properties.Add(Storage.Properties.REQUESTED_ITEMS, new List<RequestItem>());
            properties.Add(Storage.Properties.PROVIDER_LIST, new List<IActorRef>());
            properties.Add(Storage.Properties.STOCK_FOR, stockElement.Article.Name);

            return new StorageBehaviour(properties);
        }
        
        /// <summary>
        /// Has to be Stroage Agent
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Storage.Instruction.RequestArticle m:
                    RequestArticle((Storage)agent, m.GetObjectFromMessage); break;
                case Storage.Instruction.StockRefill m:
                    StockRefill((Storage)agent, m.GetObjectFromMessage); break;
                case Storage.Instruction.ResponseFromProduction m:
                    ResponseFromProduction((Storage)agent, m.GetObjectFromMessage); break;
                default: return false; 
            }
            return true;
        }

        private void RequestArticle(Storage agent, RequestItem requestItem)
        {

            // debug
            agent.DebugMessage(" requests Article " + agent.Get<Stock>(Storage.Properties.STOCK_ELEMENT).Name + " from Storage Agent ->" + agent.Sender.Path.Name);

            // try to make Reservation
            var stockReservation = Storage.MakeReservationFor(agent, requestItem);
            if (!stockReservation.IsInStock)
            {
                // add to Request queue if not in Stock
                agent.Get<List<RequestItem>>(Storage.Properties.REQUESTED_ITEMS).Add(requestItem);
            }
            // Create Callback // Probably not required here
            agent.Send(Dispo.Instruction.ResponseFromStock.Create(stockReservation, agent.Sender));
        }

        public void StockRefill(Storage agent,Guid exchangeId)
        {
            var stockElement = agent.Get<Stock>(Storage.Properties.STOCK_ELEMENT);
            // TODO: Retrun Request Itme with id of Stock Exchange
            var stockExchange = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);
            
            // stock Income 
            agent.DebugMessage(" income " + stockElement.Article.Name + " quantity " + stockExchange.Quantity + " added to Stock");
            stockElement.Current += stockExchange.Quantity;
            // change element State to Finish
            stockExchange.State = State.Finished;
            //stockExchange.RequiredOnTime = (int)Context.TimePeriod;
            stockExchange.Time = (int)agent.CurrentTime;

            var requestedItems = agent.Get<List<RequestItem>>(Storage.Properties.REQUESTED_ITEMS);
            // no Items to be served.
            if (!requestedItems.Any()) return;
            
            // Try server all Nonserved Items.
            foreach (var request in requestedItems.OrderBy(x => x.DueTime).ToList()) // .Where( x => x.DueTime <= Context.TimePeriod))
            {
                var notServed = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == request.StockExchangeId);
                if (notServed != null) { notServed.State = State.Finished; notServed.Time = (int)agent.CurrentTime; }
                else throw new Exception("No StockExchange found");
                agent.Send(Dispo.Instruction.RequestProvided.Create(request, request.Requester));
                requestedItems.Remove(request);
            }
        }

        //private void ResponseFromProduction(RequestItem item)
        private void ResponseFromProduction(Storage agent, RequestItem requestItem)
        {
            var stockElement = agent.Get<Stock>(Storage.Properties.STOCK_ELEMENT);
            var providerList = agent.Get<List<IActorRef>>(Storage.Properties.PROVIDER_LIST);
            var requestedItems = agent.Get<List<RequestItem>>(Storage.Properties.REQUESTED_ITEMS);

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
            Storage.ProvideArticle(agent, requestProvidable, requestedItems);
        }
    }
}
