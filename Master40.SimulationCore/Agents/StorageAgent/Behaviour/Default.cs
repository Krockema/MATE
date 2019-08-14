using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.StorageAgent.Types;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static FArticles;
using static FProductionResults;
using static FStockReservations;
using static FUpdateSimulationWorkProviders;
using static FUpdateStockValues;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        public Default(M_Stock stockElement, SimulationType simType) : base(simulationType: simType)
        {
            _stockElement = stockElement;
            _stockElement.StockExchanges = new List<T_StockExchange> {
                                           // Initial Value 
                                                new T_StockExchange
                                                {
                                                    StockId = stockElement.Id,
                                                    ExchangeType = ExchangeType.Insert,
                                                    Quantity = stockElement.StartValue,
                                                    State = State.Finished,
                                                    RequiredOnTime = 0,
                                                    Time = 0
                                                }};
            _stockElement.Current = stockElement.StartValue;
            _providerList = new AgentDictionary();
            _requestedArticles = new ArticleList();
        }

        internal M_Stock _stockElement { get; set; }
        internal AgentDictionary _providerList { get; set; }
        internal ArticleList _requestedArticles { get; set; }


        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Storage.Instruction.RequestArticle msg: RequestArticle(agent, msg.GetObjectFromMessage); break;
                case Storage.Instruction.StockRefill msg: StockRefill((Storage)agent, msg.GetObjectFromMessage); break;
                case Storage.Instruction.ResponseFromProduction msg: ResponseFromProduction((Storage)agent, msg.GetObjectFromMessage); break;
                case Storage.Instruction.ProvideArticleAtDue msg: ProvideArticleAtDue((Storage)agent, msg.GetObjectFromMessage); break;
                case Storage.Instruction.WithdrawlMaterial msg:
                    var stock = _stockElement;
                    Withdraw((Storage)agent, msg.GetObjectFromMessage, stock);
                    break;
                default: return false;
            }
            return true;
        }

        private void RequestArticle(Agent agent, FArticle requestItem)
        {
            agent.DebugMessage(" requests Article " + _stockElement.Name + " from Agent: " + agent.Sender.Path.Name);

            // try to make Reservation
            var item = requestItem.UpdateStockExchangeId(Guid.NewGuid()).UpdateDispoRequester(agent.Sender);
            var stockReservation = MakeReservationFor(agent, item);
            if (!stockReservation.IsInStock)
            {
                // add to Request queue if not in Stock
                _requestedArticles.Add(item);
            }
            // Create Callback
            agent.Send(Dispo.Instruction.ResponseFromStock.Create(stockReservation, agent.Sender));
        }

        public void StockRefill(Storage agent, Guid exchangeId)
        {
            // TODO: Retrun Request Itme with id of Stock Exchange
            var stockExchange = _stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);

            // stock Income 
            agent.DebugMessage(" income " + _stockElement.Article.Name + " quantity " + stockExchange.Quantity + " added to Stock");
            _stockElement.Current += stockExchange.Quantity;
            LogValueChange(agent, _stockElement.Article, Convert.ToDouble(_stockElement.Current) * Convert.ToDouble(_stockElement.Article.Price));

            // change element State to Finish
            stockExchange.State = State.Finished;
            //stockExchange.RequiredOnTime = (int)Context.TimePeriod;
            stockExchange.Time = (int)agent.CurrentTime;

            // no Items to be served.
            if (!_requestedArticles.Any()) return;

            // Try server all Nonserved Items.
            foreach (var request in _requestedArticles.OrderBy(x => x.DueTime).ToList()) // .Where( x => x.DueTime <= Context.TimePeriod))
            {
                var notServed = _stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == request.StockExchangeId);
                if (notServed == null) throw new Exception("No StockExchange found");

                notServed.State = State.Finished;
                notServed.Time = (int)agent.CurrentTime;

                agent.Send(BasicInstruction.ProvideArticle.Create(request, request.DispoRequester, false));
                _requestedArticles.Remove(request);
            }
        }


        //private void ResponseFromProduction(RequestItem item)
        private void ResponseFromProduction(Storage agent, FProductionResult productionResult)
        {
            agent.DebugMessage("Production Agent Finished Work: " + agent.Sender.Path.Name);

            // Add the Produced item to Stock
            _stockElement.Current += productionResult.Amount;
            LogValueChange(agent, _stockElement.Article, Convert.ToDouble(_stockElement.Current) * Convert.ToDouble(_stockElement.Article.Price));


            var stockExchange = new T_StockExchange
            {
                StockId = _stockElement.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = 1,
                State = State.Finished,
                RequiredOnTime = (int)agent.CurrentTime,
                Time = (int)agent.CurrentTime
            };
            _stockElement.StockExchanges.Add(stockExchange);

            _providerList.Add(agent.Sender, productionResult.ArticleKey);
            // Check if the most Important Request can be provided.
            var requestProvidable = _requestedArticles.FirstOrDefault(x => x.DueTime == _requestedArticles.Min(r => r.DueTime));
            // TODO: Prove if quantity check is required.

            if (requestProvidable.IsHeadDemand && requestProvidable.DueTime > agent.CurrentTime) { return; }
            // else
            ProvideArticle(agent, requestProvidable, _requestedArticles);
        }

        private void ProvideArticleAtDue(Storage agent, Guid articleKey)
        {
            var requestProvidable = _requestedArticles.GetByKey(articleKey);
            if (requestProvidable != null)
            {
                ProvideArticle(agent, requestProvidable, _requestedArticles);
            }
        }

        private void Withdraw(Storage agent, Guid exchangeId, M_Stock stockElement)
        {
            var item = stockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == exchangeId);
            if (item == null) throw new Exception("No StockExchange found");
            LogValueChange(agent, stockElement.Article, Convert.ToDouble(stockElement.Current) * Convert.ToDouble(stockElement.Article.Price));

            item.State = State.Finished;
            item.Time = (int)agent.CurrentTime;
        }

        private void ProvideArticle(Storage agent, FArticle requestProvidable, List<FArticle> requestedArticles)
        {
            if (requestProvidable.Quantity <= _stockElement.Current)
            {
                //TODO: Create Actor for Withdrawl remove the item on DueTime from Stock.

                if (requestProvidable.IsHeadDemand)
                {
                    Withdraw(agent, requestProvidable.StockExchangeId, _stockElement);
                }
                else
                {
                    LogValueChange(agent, _stockElement.Article, Convert.ToDouble(_stockElement.Current) * Convert.ToDouble(_stockElement.Article.Price));
                }

                if (requestProvidable.ProviderList.Count == 0)
                {
                    requestProvidable = requestProvidable.UpdateProviderList(new List<IActorRef>(_providerList.ToSimpleList()));
                    _providerList.Clear();
                }

                agent.DebugMessage("------------->> items in STOCK: " + _stockElement.Current + " Items Requested " + requestProvidable.Quantity);
                // Reduce Stock 
                _stockElement.Current = _stockElement.Current - requestProvidable.Quantity;

                // Remove from Requester List.
                _requestedArticles.RemoveByKey(requestProvidable.Key);

                requestProvidable = requestProvidable.SetProvided;
                // Create Callback for Production
                agent.Send(BasicInstruction.ProvideArticle.Create(requestProvidable, requestProvidable.DispoRequester, false));


                // Update Work Item with Provider For
                // TODO

                var pub = new FUpdateSimulationWorkProvider(requestProvidable.ProviderList
                                                        , requestProvidable.DispoRequester.Path.Uid.ToString()
                                                        , requestProvidable.DispoRequester.Path.Name
                                                        , requestProvidable.IsHeadDemand
                                                        , requestProvidable.CustomerOrderId);
                agent.Context.System.EventStream.Publish(pub);


                // Statistics.UpdateSimulationWorkSchedule(requestProvidable.ProviderList, requestProvidable.Requester, requestProvidable.OrderId);
                // ProviderList.Clear();
            }
            else
            {
                agent.DebugMessage("Item will be late..............................");
            }
        }

        internal FStockReservation MakeReservationFor(Agent agent, FArticle request)
        {
            var inStock = false;
            var quantity = 0;

            //StockReservation stockReservation = new StockReservation { DueTime = request.DueTime };
            var withdrawl = _stockElement.StockExchanges
                                .Where(x => x.RequiredOnTime <= request.DueTime &&
                                            x.State != State.Finished &&
                                            x.ExchangeType == ExchangeType.Withdrawal)
                                .Sum(x => x.Quantity);
            // Element is NOT in Stock
            // Create Purchase if Required.
            var purchaseOpen = _stockElement.StockExchanges
                .Any(x => x.State != State.Finished && x.ExchangeType == ExchangeType.Insert);
            var required = ((_stockElement.Current - withdrawl - request.Quantity));
            if (required < _stockElement.Min && _stockElement.Article.ToPurchase && !purchaseOpen)
            {
                CreatePurchase(agent, _stockElement);
                purchaseOpen = true;
                agent.DebugMessage(" Created purchase for " + _stockElement.Article.Name);
            }

            // Create Reservation Item
            if (required > 0)
            {
                inStock = true;
                quantity = request.Quantity;
                _stockElement.Current -= request.Quantity;
            }
            var stockReservation = new FStockReservation(quantity, purchaseOpen, inStock, request.DueTime, request.StockExchangeId);

            //Create Stockexchange for Reservation
            _stockElement.StockExchanges.Add(
                new T_StockExchange
                {
                    TrakingGuid = request.StockExchangeId,
                    StockId = _stockElement.Id,
                    ExchangeType = ExchangeType.Withdrawal,
                    Quantity = request.Quantity,
                    Time = (int)(agent.CurrentTime),
                    State = stockReservation.IsInStock ? State.Finished : State.Created,
                    RequiredOnTime = (int)request.DueTime,
                }
            );
            return stockReservation;
        }

        internal void CreatePurchase(Agent agent, M_Stock stockElement)
        {

            var time = stockElement.Article
                                    .ArticleToBusinessPartners
                                    .Single(x => x.BusinessPartner.Kreditor)
                                    .DueTime;
            var stockExchange = new T_StockExchange
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
            agent.Send(Storage.Instruction.StockRefill.Create(stockExchange.TrakingGuid, agent.Context.Self), time);
        }

        internal void LogValueChange(Agent agent, M_Article article, double value)
        {
            var pub = new FUpdateStockValue(article.Name
                                            , value
                                            , article.ArticleType.Name);
            agent.Context.System.EventStream.Publish(pub);
        }
    }
}
