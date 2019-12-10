using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.StorageAgent.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Akka;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Nominal;
using static FArticleProviders;
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
            _stockManager = new StockManager(stockElement);
            _requestedArticles = new ArticleList();
        }

        internal StockManager _stockManager { get; set; }
        internal ArticleList _requestedArticles { get; set; }


        public override bool Action(object message)
        {
            switch (message)
            {
                case Storage.Instruction.RequestArticle msg: RequestArticle(requestItem: msg.GetObjectFromMessage); break;
                case Storage.Instruction.StockRefill msg: RefillFromPurchase(exchangeId: msg.GetObjectFromMessage); break;
                case Storage.Instruction.ResponseFromProduction msg: ResponseFromProduction(productionResult: msg.GetObjectFromMessage); break;
                case Storage.Instruction.ProvideArticleAtDue msg: ProvideArticleAtDue(articleKey: msg.GetObjectFromMessage); break;
                case Storage.Instruction.WithdrawArticle msg: WithdrawArticle(exchangeId: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void RequestArticle(FArticle requestItem)
        {
            Agent.DebugMessage(msg: " requests Article " + _stockManager.Name + " from Agent: " + Agent.Sender.Path.Name);

            // try to make Reservation
            var item = requestItem.UpdateStockExchangeId(i: Guid.NewGuid()).UpdateDispoRequester(r: Agent.Sender);
            var stockReservation = MakeReservationFor(request: item);
            if (!stockReservation.IsInStock || item.Article.ToBuild)
            {
                // add to Request queue if not in Stock
                _requestedArticles.Add(item: item);
            }
            // Create Callback
            Agent.Send(instruction: Dispo.Instruction.ResponseFromStock.Create(message: stockReservation, target: Agent.Sender));
        }

        public void RefillFromPurchase(Guid exchangeId)
        {
            // TODO: Return exchangeId of Stock Exchange
            var stockExchange = _stockManager.GetStockExchangeByTrackingGuid(exchangeId);
          
            // stock Income 
            Agent.DebugMessage(msg: " income " + _stockManager.Article.Name + " quantity " + stockExchange.Quantity + " added to Stock");
            
            // change element State to Finish
            stockExchange.State = State.Finished;
            //stockExchange.RequiredOnTime = (int)Context.TimePeriod;
            stockExchange.Time = (int)Agent.CurrentTime;
            _stockManager.AddToStock(stockExchange);
            LogValueChange(article: _stockManager.Article, value: Convert.ToDouble(value: _stockManager.Current) * Convert.ToDouble(value: _stockManager.Price));

            // no Items to be served.
            if (!_requestedArticles.Any()) return;

            // Try server all not served Items. // TODO: Is it Correct to select Items by DueTime and not its Priority ? Make it Changeable ?  
            foreach (var request in _requestedArticles.OrderBy(keySelector: x => x.DueTime).ToList()) 
            {
                var notServed = _stockManager.GetStockExchangeByTrackingGuid(request.StockExchangeId);
                if (notServed == null) throw new Exception(message: "No StockExchange found");

                notServed.State = State.Finished;
                notServed.Time = (int)Agent.CurrentTime;

                Agent.Send(instruction: BasicInstruction.ProvideArticle
                                                        .Create(message: new FArticleProvider(articleKey: request.Key
                                                                                             ,articleName: request.Article.Name
                                                                                             , articleFinishedAt: Agent.CurrentTime
                                                                                             ,stockExchangeId: request.StockExchangeId
                                                                                             , provider: new List<Guid>(new [] { stockExchange.TrackingGuid }))
                                                               , target: request.DispoRequester
                                                              , logThis: false));
                _requestedArticles.Remove(item: request);
            }
        }

        private void ResponseFromProduction(FProductionResult productionResult)
        {
            // Add the Produced item to Stock
            Agent.DebugMessage(msg: $"{productionResult.Amount} {_stockManager.Name} was send from {Agent.Sender.Path.Name}");
            var stockExchange = new T_StockExchange
            {
                StockId = _stockManager.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = productionResult.Amount,
                State = State.Finished,
                RequiredOnTime = (int)Agent.CurrentTime,
                Time = (int)Agent.CurrentTime,
                ProductionArticleKey = productionResult.Key
            };
            _stockManager.AddToStock(stockExchange);
            _stockManager.StockExchanges.Add(stockExchange);

            // log Changes 
            LogValueChange(article: _stockManager.Article, value: Convert.ToDouble(value: _stockManager.Current) * Convert.ToDouble(value: _stockManager.Price));

            // Check if the most Important Request can be provided.
            var mostUrgentRequest = _requestedArticles.First(predicate: x => x.DueTime == _requestedArticles.Min(selector: r => r.DueTime));

            //TODO: might be a problem 
            if (mostUrgentRequest.IsHeadDemand && mostUrgentRequest.DueTime > Agent.CurrentTime)
            {
                Agent.DebugMessage(msg: $"{_stockManager.Name} {mostUrgentRequest.Key} finished before due. CostumerOrder {mostUrgentRequest.CustomerOrderId} will ask at {mostUrgentRequest.DueTime} for article.");
                return;
            }
            // else if
            if (mostUrgentRequest.Quantity <= _stockManager.Current)
            {
                Agent.DebugMessage(msg: $"{_stockManager.Name} {mostUrgentRequest.Key} is providable {mostUrgentRequest.Quantity} does match current Stock amount of {_stockManager.Current}.");
                ProvideArticle(articleKey: mostUrgentRequest.Key);
            }
        }

        private void ProvideArticleAtDue(Guid articleKey)
        {
            Agent.DebugMessage(msg: $"{_stockManager.Name} {articleKey} shall be provided at due {_stockManager.Current}.");
            ProvideArticle(articleKey: articleKey);
        }

        private void ProvideArticle(Guid articleKey)
        {
            var article = _requestedArticles.GetByKey(articleKey);

            if (article.Quantity <= _stockManager.Current)
            {
                var providerList = _stockManager.GetProviderGuidsFor(new Quantity(article.Quantity));
                article = article.UpdateProviderList(p: providerList.Select(x => x.ProductionArticleKey).ToList());
                article = article.UpdateFinishedAt(providerList.Max(x => x.Time));

                Agent.DebugMessage(msg: "------------->> items in STOCK: " + _stockManager.Current + " Items Requested " + article.Quantity);
                // Reduce Stock 
                // Done in GetProviderGuidsFor //TODO: find better Naming or split methods

                // Remove from Requester List
                //TODO might be to early, handle headdemands different?
                _requestedArticles.RemoveByKey(key: article.Key);
                
                // Agent Sender can be a DispoAgent (ProvideArticleAtDue) or a ProductionAgent (ResponseFromProduction)
                Agent.DebugMessage(msg: $"Provide Article: {article.Article.Name} {article.Key} from {Agent.Sender}");

                // Create Callback for Production
                Agent.Send(instruction: BasicInstruction.ProvideArticle.Create(message: new FArticleProvider(articleKey: article.Key
                                                                                                           , articleName: article.Article.Name
                                                                                                           , stockExchangeId: article.StockExchangeId
                                                                                                           , articleFinishedAt: article.FinishedAt
                                                                                                           , provider: article.ProviderList)
                                                                              , target: article.DispoRequester
                                                                             , logThis: false));

                // Update Work Item with Provider For
                // TODO



                var pub = new FUpdateSimulationWorkProvider(fArticleProviderKeys: article.ProviderList
                                                        , requestAgentId: article.DispoRequester.Path.Uid.ToString()
                                                        , requestAgentName: article.DispoRequester.Path.Name
                                                        , isHeadDemand: article.IsHeadDemand
                                                        , customerOrderId: article.CustomerOrderId);
                Agent.Context.System.EventStream.Publish(@event: pub);
            }
            else
            {
                Agent.DebugMessage(msg: "Item will be late..............................");
            }
        }

        internal FStockReservation MakeReservationFor(FArticle request)
        {
            var inStock = false;
            var quantity = 0;
            var time = request.DueTime;

            var withdraw = _stockManager.StockExchanges
                                .Where(predicate: x => x.RequiredOnTime <= request.DueTime &&
                                            x.State != State.Finished &&
                                            x.ExchangeType == ExchangeType.Withdrawal)
                                .Sum(selector: x => x.Quantity);
            // Element is NOT in Stock
            // Create Purchase if Required.
            var purchaseOpen = _stockManager.StockExchanges
                .Any(predicate: x => x.State != State.Finished && x.ExchangeType == ExchangeType.Insert);
            var required = ((_stockManager.Current - withdraw - request.Quantity));
            if (required < _stockManager.Stock.Min && _stockManager.Article.ToPurchase && !purchaseOpen)
            {
                var timeToDelivery = CreatePurchase(stockElement: _stockManager.Stock);
                time = Agent.CurrentTime + timeToDelivery;
                purchaseOpen = true;
                Agent.DebugMessage(msg: $"Created purchase for {_stockManager.Article.Name}");
            }

            // Create Reservation Item
            if (required > 0)
            {
                inStock = true;
                quantity = request.Quantity;
                //TODO Check Correctness
                _stockManager.GetProviderGuidsFor(new Quantity(request.Quantity));
            }
            var stockReservation = new FStockReservation(quantity: quantity, isPurchased: purchaseOpen, isInStock: inStock, dueTime: time, trackingId: request.StockExchangeId);

            //Create Stockexchange for Reservation
            _stockManager.StockExchanges.Add(
                item: new T_StockExchange
                {
                    TrackingGuid = request.StockExchangeId,
                    StockId = _stockManager.Id,
                    ExchangeType = ExchangeType.Withdrawal,
                    Quantity = request.Quantity,
                    Time = (int)(Agent.CurrentTime),
                    State = stockReservation.IsInStock ? State.Finished : State.Created,
                    RequiredOnTime = (int)request.DueTime,
                }
            );
            return stockReservation;
        }

        internal long CreatePurchase(M_Stock stockElement)
        {
            var time = stockElement.Article
                                    .ArticleToBusinessPartners
                                    .Single(predicate: x => x.BusinessPartner.Kreditor)
                                    .TimeToDelivery;
            var stockExchange = new T_StockExchange
            {
                StockId = stockElement.Id,
                ExchangeType = ExchangeType.Insert,
                State = State.Created,
                Time = (int)(Agent.CurrentTime),
                Quantity = stockElement.Article.Stock.Max - stockElement.Article.Stock.Min,
                RequiredOnTime = (int)(Agent.CurrentTime) + time,
                TrackingGuid = Guid.NewGuid()
            };

            _stockManager.StockExchanges.Add(item: stockExchange);
            // TODO Needs logic if more Kreditors are Added.
            // TODO start CreatePurchase later if materials are needed later
            Agent.Send(instruction: Storage.Instruction.StockRefill.Create(message: stockExchange.TrackingGuid, target: Agent.Context.Self), waitFor: time);

            return time;
        }
        private void WithdrawArticle(Guid exchangeId)
        {
            var stockExchange = _stockManager.GetStockExchangeByTrackingGuid(exchangeId);
            if (stockExchange == null) throw new Exception(message: "No StockExchange was found");
            LogValueChange(article: _stockManager.Article, value: Convert.ToDouble(value: _stockManager.Current) * Convert.ToDouble(value: _stockManager.Price));

            stockExchange.State = State.Finished;
            stockExchange.Time = (int)Agent.CurrentTime;

            Agent.DebugMessage(msg: $"Withdraw stock exchange {stockExchange.TrackingGuid} for article {stockExchange.Quantity} {_stockManager.Name}");

        }

        internal void LogValueChange(M_Article article, double value)
        {
            var pub = new FUpdateStockValue(stockName: article.Name
                                            , newValue: value
                                            , articleType: article.ArticleType.Name);
            Agent.Context.System.EventStream.Publish(@event: pub);
        }
    }
}
