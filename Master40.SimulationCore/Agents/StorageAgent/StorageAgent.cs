using Akka.Actor;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    public partial class StorageAgent : Agent
    {
        // Statistic 
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new StorageAgent(actorPaths, time, debug));
        }

        public StorageAgent(ActorPaths actorPaths, long time, bool debug) : base(actorPaths, time, debug)
        {
            
        }

        internal static void ProvideArticleAtDue(Agent agent, RequestItem requestItem)
        {
            if (requestItem == null)
                throw new InvalidCastException(agent.Name + " failed to Cast RequestItem on Instruction.ObjectToProcess");
            // discard request, if the item has already been provided.

            var requestedItems = agent.Get<List<RequestItem>>(StorageBehaviour.RequestedItems);
            var requestProvidable = requestedItems.FirstOrDefault(r => r.Requester == requestItem.Requester);
            if (requestProvidable != null)
            {
                StorageAgent.ProvideArticle(agent, requestProvidable, requestedItems);
            }
        }

        internal static void ProvideArticle(Agent agent, RequestItem requestProvidable, List<RequestItem> requestedItems)
        {
            var stockElement = agent.Get<Stock>(StorageBehaviour.StockElement);
            if (requestProvidable.Quantity <= stockElement.Current)
            {
                var providerList = agent.Get<List<IActorRef>>(StorageBehaviour.ProviderList);
                //TODO: Create Actor for Withdrawl remove the item on DueTime from Stock.

                if (requestProvidable.IsHeadDemand)
                    Withdraw(agent, requestProvidable.StockExchangeId);

                if (requestProvidable.ProviderList.Count == 0)
                {
                    requestProvidable = requestProvidable.UpdateProviderList(new List<IActorRef>(providerList));
                    providerList.Clear();
                }

                agent.DebugMessage("------------->> items in STOCK: " + stockElement.Current + " Items Requested " + requestProvidable.Quantity);
                // Reduce Stock 
                stockElement.Current = stockElement.Current - requestProvidable.Quantity;

                // Remove from Requester List.
                requestedItems.Remove(requestedItems.Single(x => x.Key == requestProvidable.Key));
                
                requestProvidable = requestProvidable.SetProvided;
                // Create Callback for Production
                agent.Send(DispoAgent.Instruction.RequestProvided.Create(requestProvidable, requestProvidable.Requester));


                // Update Work Item with Provider For
                // TODO
                // Statistics.UpdateSimulationWorkSchedule(requestProvidable.ProviderList, requestProvidable.Requester, requestProvidable.OrderId);
                //ProviderList.Clear();
            }
            else
            {
                agent.DebugMessage("Item will be late..............................");
            }
        }

        internal static void Withdraw(Agent agent, Guid exchangeId)
        {
            var stockElement = agent.Get<Stock>(StorageBehaviour.StockElement);
            var item = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);
            if (item != null) { item.State = State.Finished; item.Time = (int)agent.CurrentTime; }
            else throw new Exception("No StockExchange found");
        }

        internal static StockReservation MakeReservationFor(Agent agent, RequestItem request)
        {
            request = request.UpdateStockExchangeId(Guid.NewGuid());
            var inStock = false;
            var quantity = 0;


            //StockReservation stockReservation = new StockReservation { DueTime = request.DueTime };
            var stockElement = agent.Get<Stock>(StorageBehaviour.StockElement);
            var withdrawl = stockElement.StockExchanges
                                .Where(x => x.RequiredOnTime <= request.DueTime &&
                                            x.State != State.Finished &&
                                            x.ExchangeType == ExchangeType.Withdrawal)
                                .Sum(x => x.Quantity);
            // Element is NOT in Stock
            // Create Order if Required.
            var purchaseOpen = stockElement.StockExchanges
                .Any(x => x.State != State.Finished && x.ExchangeType == ExchangeType.Insert);
            var min = ((stockElement.Current - withdrawl - request.Quantity) < stockElement.Min);
            if (min && stockElement.Article.ToPurchase && !purchaseOpen)
            {
                StorageAgent.CreatePurchase(agent, stockElement);
                purchaseOpen = true;
                agent.DebugMessage(" Created purchase for " + stockElement.Article.Name);
            }

            //if ((StockElement.Current + insert - withdrawl - request.Quantity) < 0)
            if ((stockElement.Current - withdrawl - request.Quantity) > 0)
            {
                inStock = true;
                quantity = request.Quantity;
                stockElement.Current -= request.Quantity;
            }

            var stockReservation = new StockReservation(quantity, purchaseOpen, inStock, request.DueTime);


            //Create Reservation
            stockElement.StockExchanges.Add(
                new StockExchange
                {
                    TrakingGuid = request.StockExchangeId,
                    StockId = stockElement.Id,
                    ExchangeType = ExchangeType.Withdrawal,
                    Quantity = request.Quantity,
                    Time = (int)(agent.CurrentTime),
                    State = stockReservation.IsInStock ? State.Finished : State.Created,
                    RequiredOnTime = (int)request.DueTime,
                }
            );
            return stockReservation;
        }

        internal static void CreatePurchase(Agent agent, Stock stockElement)
        {

            var time = stockElement.Article
                                    .ArticleToBusinessPartners
                                    .Single(x => x.BusinessPartner.Kreditor)
                                    .DueTime;
            var stockExchange = new StockExchange
            {
                StockId = stockElement.Id,
                ExchangeType = ExchangeType.Insert,
                State = State.Created,
                Time = (int)(agent.CurrentTime),
                Quantity = stockElement.Article.Stock.Max - stockElement.Article.Stock.Min,
                RequiredOnTime = (int)(agent.CurrentTime) + time,
                TrakingGuid = Guid.NewGuid()
            };

            stockElement.StockExchanges.Add(stockExchange);
            // TODO needs logic if more Kreditors are Added.
            agent.Send(Instruction.StockRefill.Create(stockExchange.TrakingGuid, agent.Context.Self), time);
        }
    }
}
