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
    public partial class Storage : Agent
    {

        // Statistic 
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Storage(actorPaths, time, debug, principal));
        }

        public Storage(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            
        }

        protected override void OnInit(IBehaviour o)
        {
            o.Properties.TryGetValue(Properties.STOCK_ELEMENT, out object stock);
            DebugMessage("Current: " + ((Stock)stock).Current);
        }

       
        internal FStockReservation MakeReservationFor(Agent agent, FRequestItem request)
        {
            var inStock = false;
            var quantity = 0;


            //StockReservation stockReservation = new StockReservation { DueTime = request.DueTime };
            var stockElement = agent.Get<Stock>(Properties.STOCK_ELEMENT);
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
                CreatePurchase(agent, stockElement);
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

            var stockReservation = new FStockReservation(quantity, purchaseOpen, inStock, request.DueTime);


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

        internal void CreatePurchase(Agent agent, Stock stockElement)
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
