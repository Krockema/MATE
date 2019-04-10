using System;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.StorageAgent
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
            // Create Purchase if Required.
            var purchaseOpen = stockElement.StockExchanges
                .Any(x => x.State != State.Finished && x.ExchangeType == ExchangeType.Insert);
            var required = ((stockElement.Current - withdrawl - request.Quantity));
            if (required < stockElement.Min && stockElement.Article.ToPurchase && !purchaseOpen)
            {
                CreatePurchase(agent, stockElement);
                purchaseOpen = true;
                agent.DebugMessage(" Created purchase for " + stockElement.Article.Name);
            }

            // Create Reservation Item
            if (required > 0)
            {
                inStock = true;
                quantity = request.Quantity;
                stockElement.Current -= request.Quantity;
            }
            var stockReservation = new FStockReservation(quantity, purchaseOpen, inStock, request.DueTime, request.StockExchangeId);

            //Create Stockexchange for Reservation
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

        internal void LogValueChange(Agent agent, Article article, double value)
        {
            var pub = new UpdateStockValues(article.Name
                                            , value
                                            , article.ArticleType.Name);
            agent.Context.System.EventStream.Publish(pub);
        }
    }
}
