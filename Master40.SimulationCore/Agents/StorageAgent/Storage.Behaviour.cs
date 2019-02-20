using Akka.Actor;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using static Master40.SimulationCore.Agents.Storage.Instruction;
using static Master40.SimulationCore.Agents.Storage.Properties;

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

            properties.Add(STOCK_ELEMENT, stockElement);
            properties.Add(REQUESTED_ITEMS, new List<FRequestItem>());
            properties.Add(PROVIDER_LIST, new List<IActorRef>());
            properties.Add(STOCK_FOR, stockElement.Article.Name);

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
                case RequestArticle msg: RequestArticle((Storage)agent, msg.GetObjectFromMessage); break;
                case StockRefill msg: StockRefill((Storage)agent, msg.GetObjectFromMessage); break;
                case ResponseFromProduction msg: ResponseFromProduction((Storage)agent, msg.GetObjectFromMessage); break;
                case ProvideArticleAtDue msg: ProvideArticleAtDue((Storage)agent, msg.GetObjectFromMessage); break;
                case WithdrawlMaterial msg:
                    var stock = agent.Get<Stock>(STOCK_ELEMENT);
                    Withdraw((Storage)agent, msg.GetObjectFromMessage, stock);
                    break;
                default: return false; 
            }
            return true;
        }

        private void RequestArticle(Storage agent, FRequestItem requestItem)
        {

            // debug
            agent.DebugMessage(" requests Article " + agent.Get<Stock>(STOCK_ELEMENT).Name + " from Storage Agent ->" + agent.Sender.Path.Name);

            // try to make Reservation
            var item = requestItem.UpdateStockExchangeId(Guid.NewGuid()).UpdateDispoRequester(agent.Sender);
            var stockReservation = agent.MakeReservationFor(agent, item);
            if (!stockReservation.IsInStock)
            {
                // add to Request queue if not in Stock
                agent.Get<List<FRequestItem>>(REQUESTED_ITEMS).Add(item);
            }
            // Create Callback // Probably not required here
            agent.Send(Dispo.Instruction.ResponseFromStock.Create(stockReservation, agent.Sender));
        }

        public void StockRefill(Storage agent,Guid exchangeId)
        {
            var stockElement = agent.Get<Stock>(STOCK_ELEMENT);
            // TODO: Retrun Request Itme with id of Stock Exchange
            var stockExchange = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);
            
            // stock Income 
            agent.DebugMessage(" income " + stockElement.Article.Name + " quantity " + stockExchange.Quantity + " added to Stock");
            stockElement.Current += stockExchange.Quantity;
            agent.LogValueChange(agent, stockElement.Article, Convert.ToDouble(stockElement.Current) * Convert.ToDouble(stockElement.Article.Price));



            // change element State to Finish
            stockExchange.State = State.Finished;
            //stockExchange.RequiredOnTime = (int)Context.TimePeriod;
            stockExchange.Time = (int)agent.CurrentTime;

            var requestedItems = agent.Get<List<FRequestItem>>(REQUESTED_ITEMS);
            // no Items to be served.
            if (!requestedItems.Any()) return;
            
            // Try server all Nonserved Items.
            foreach (var request in requestedItems.OrderBy(x => x.DueTime).ToList()) // .Where( x => x.DueTime <= Context.TimePeriod))
            {
                var notServed = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == request.StockExchangeId);
                if (notServed != null) { notServed.State = State.Finished; notServed.Time = (int)agent.CurrentTime; }
                else throw new Exception("No StockExchange found");
                agent.Send(Dispo.Instruction.RequestProvided.Create(request, request.DispoRequester));
                requestedItems.Remove(request);
            }
        }


        //private void ResponseFromProduction(RequestItem item)
        private void ResponseFromProduction(Storage agent, FRequestItem requestItem)
        {
            var stockElement = agent.Get<Stock>(STOCK_ELEMENT);
            var providerList = agent.Get<List<IActorRef>>(PROVIDER_LIST);
            var requestedItems = agent.Get<List<FRequestItem>>(REQUESTED_ITEMS);

            if (requestItem == null)
            {
                throw new InvalidCastException(agent.Name + " failed to Cast ProductionAgent on Instruction.ObjectToProcess");
            }

            agent.DebugMessage("Production Agent Finished Work: " + agent.Sender.Path.Name);


            // Add the Produced item to Stock
            stockElement.Current++;
            agent.LogValueChange(agent, stockElement.Article, Convert.ToDouble(stockElement.Current) * Convert.ToDouble(stockElement.Article.Price));


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
            ProvideArticle(agent, requestProvidable, requestedItems);
        }

        private void ProvideArticleAtDue(Storage agent, FRequestItem requestItem)
        {
            if (requestItem == null)
                throw new InvalidCastException(agent.Name + " failed to Cast RequestItem on Instruction.ObjectToProcess");
            // discard request, if the item has already been provided.

            var requestedItems = agent.Get<List<FRequestItem>>(REQUESTED_ITEMS);
            var requestProvidable = requestedItems.FirstOrDefault(r => r.Key == requestItem.Key);
            if (requestProvidable != null)
            {
                ProvideArticle(agent, requestProvidable, requestedItems);
            }
        }

        private void Withdraw(Storage agent, Guid exchangeId, Stock stockElement)
        {
            var item = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);
            if (item == null) throw new Exception("No StockExchange found");
            agent.LogValueChange(agent, stockElement.Article, Convert.ToDouble(stockElement.Current) * Convert.ToDouble(stockElement.Article.Price));

            item.State = State.Finished;
            item.Time = (int)agent.CurrentTime;
        }

        private void ProvideArticle(Storage agent, FRequestItem requestProvidable, List<FRequestItem> requestedItems)
        {
            var stockElement = agent.Get<Stock>(STOCK_ELEMENT);
            if (requestProvidable.Quantity <= stockElement.Current)
            {
                var providerList = agent.Get<List<IActorRef>>(PROVIDER_LIST);
                //TODO: Create Actor for Withdrawl remove the item on DueTime from Stock.
                var withdrawn = false;
                if (requestProvidable.IsHeadDemand) {
                    withdrawn = true;
                    Withdraw(agent, requestProvidable.StockExchangeId, stockElement);
                }
                if (requestProvidable.ProviderList.Count == 0)
                {
                    requestProvidable = requestProvidable.UpdateProviderList(new List<IActorRef>(providerList));
                    providerList.Clear();
                }

                agent.DebugMessage("------------->> items in STOCK: " + stockElement.Current + " Items Requested " + requestProvidable.Quantity);
                // Reduce Stock 
                if (!withdrawn)
                {
                    stockElement.Current = stockElement.Current - requestProvidable.Quantity;
                    agent.LogValueChange(agent, stockElement.Article, Convert.ToDouble(stockElement.Current) * Convert.ToDouble(stockElement.Article.Price));
                }
                // Remove from Requester List.
                requestedItems.Remove(requestedItems.Single(x => x.Key == requestProvidable.Key));

                requestProvidable = requestProvidable.SetProvided;
                // Create Callback for Production
                agent.Send(Dispo.Instruction.RequestProvided.Create(requestProvidable, requestProvidable.DispoRequester));


                // Update Work Item with Provider For
                // TODO
                
                var pub = new UpdateSimulationWorkProvider(requestProvidable.ProviderList
                                                        , requestProvidable.DispoRequester.Path.Uid.ToString()
                                                        , requestProvidable.DispoRequester.Path.Name
                                                        , requestProvidable.IsHeadDemand
                                                        , requestProvidable.OrderId);
                agent.Context.System.EventStream.Publish(pub);


                // Statistics.UpdateSimulationWorkSchedule(requestProvidable.ProviderList, requestProvidable.Requester, requestProvidable.OrderId);
                // ProviderList.Clear();
            }
            else
            {
                agent.DebugMessage("Item will be late..............................");
            }
        }
    }
}
