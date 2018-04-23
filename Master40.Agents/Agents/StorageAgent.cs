using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Enums;
using Master40.DB.Models;

namespace Master40.Agents.Agents
{
    public class StorageAgent : Agent
    {
        // Statistic 
        private List<Guid> ProviderList { get; set; }

        // Properties
        public Stock StockElement { get; set; }
        public string StockFor { get; }
        private List<RequestItem> RequestedItems { get; set; }

        public StorageAgent(Agent creator, string name, bool debug, Stock stockElement) : base(creator, name, debug)
        {
            StockElement = stockElement;
            StockFor = stockElement.Article.Name;
            RequestedItems = new List<RequestItem>();
            ProviderList = new List<Guid>();
            var stockExchange = new StockExchange
            {
                StockId = StockElement.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = stockElement.StartValue,
                State = State.Finished,
                RequiredOnTime = (int)Context.TimePeriod,
                Time = (int)Context.TimePeriod
            };
            StockElement.StockExchanges.Add(stockExchange);
            //Instructions = new List<Instruction>{ new Instruction{ Method = "RequestArticle", ExpectedObjecType = typeof(int) } };
        }

        public enum InstuctionsMethods
        {
            // Create and Return a Reservation for Article
            RequestArticle,
            ProvideArticleAtDue,
            ResponseFromProduction,
            StockRefill,
            WithdrawlMaterial
        }

        /// <summary>
        /// Returns the Reservation for Request
        /// ATTENTION: CAN BE - 0 -!
        /// </summary>
        /// <param name="instructionSet"></param>
        private void RequestArticle(InstructionSet instructionSet)
        {
            // debug
            DebugMessage(" requests Article " + StockElement.Name + " from Stock Agent ->" + instructionSet.SourceAgent.Name);

            // cast Request
            var request = instructionSet.ObjectToProcess as RequestItem;
            if (request == null)
                throw new InvalidCastException("Cast to Request Item Failed");


            // try to make Reservation
            var stockReservation = MakeReservationFor(request);
            if (!stockReservation.IsInStock)
            {
                // add to Request queue if not in Stock
                RequestedItems.Add(request);
            }


            // Create Callback // Probably not required here
            CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.ResponseFromStock.ToString(),
                                  objectToProcess: stockReservation, // may needs later a more complex answer for now just remove item from stock
                                      targetAgent: instructionSet.SourceAgent /*,
                                          waitFor: request.DueTime */ );  // its Source Agent becaus this message is the Answer to the Instruction set.
                                     
        }

        private void ResponseFromProduction(InstructionSet instructionSet)
        {
            var productionAgent = instructionSet.ObjectToProcess as ProductionAgent;
            if (productionAgent == null)
            {
                throw new InvalidCastException(this.Name + " failed to Cast ProductionAgent on Instruction.ObjectToProcess");
            }

            DebugMessage("Production Agent Finished Work: " + productionAgent.Name);


            // Add the Produced item to Stock
            StockElement.Current++;
            var stockExchange = new StockExchange
            {
                StockId = StockElement.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = 1,
                State = State.Finished,
                RequiredOnTime = (int)Context.TimePeriod,
                Time = (int)Context.TimePeriod
            };
            StockElement.StockExchanges.Add(stockExchange);

            ProviderList.Add(productionAgent.AgentId);
            // Check if the most Important Request can be provided.
            var requestProvidable = RequestedItems.FirstOrDefault(x => x.DueTime == RequestedItems.Min(r => r.DueTime));
            // TODO: Prove if quantity check is required.

            if (requestProvidable.IsHeadDemand && requestProvidable.DueTime > Context.TimePeriod) { return; }
            // else
            ProvideArticle(requestProvidable);

        }

        private void ProvideArticleAtDue(InstructionSet instructionSet)
        {
            var requestProvidable = instructionSet.ObjectToProcess as RequestItem;
            if (requestProvidable == null)
                throw new InvalidCastException(this.Name + " failed to Cast RequestItem on Instruction.ObjectToProcess");
            // discard request, if the item has already been provided.
            if (this.RequestedItems.Any(r => r.Requester == requestProvidable.Requester))
            {
                ProvideArticle(requestProvidable);
            }
        }

        private void ProvideArticle(RequestItem requestProvidable)
        {
            if (requestProvidable.Quantity <= StockElement.Current)
            {
                //TODO: Create Actor for Withdrawl remove the item on DueTime from Stock.

                if (requestProvidable.IsHeadDemand)
                    Withdraw(requestProvidable);

                if (requestProvidable.ProviderList.Count == 0)
                {
                    requestProvidable.ProviderList = new List<Guid>(ProviderList);
                    ProviderList.Clear();
                }

                // Reduce Stock 
                StockElement.Current = StockElement.Current - requestProvidable.Quantity;
                DebugMessage("------------->> items in STOCK: " + StockElement.Current + " Items Requested " + requestProvidable.Quantity);

                // Create Callback for Production
                CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.RequestProvided.ToString(),
                                        objectToProcess: requestProvidable, // may needs later a more complex answer for now just remove item from stock
                                        targetAgent: requestProvidable.Requester); // its Source Agent becaus this message is the Answer to the Instruction set.

                // Remove from Requester List.
                this.RequestedItems.Remove(requestProvidable);

                // Update Work Item with Provider For
                Statistics.UpdateSimulationWorkSchedule(requestProvidable.ProviderList, requestProvidable.Requester, requestProvidable.OrderId);
                //ProviderList.Clear();
            } else
            {
                DebugMessage("Item will be late..............................");
            }
        }

        private void StockRefill(InstructionSet instructionSet)
        {

            // TODO: Retrun Request Itme with id of Stock Exchange
            var stockExchange = instructionSet.ObjectToProcess as StockExchange;
            if (stockExchange == null)
            {
                throw new InvalidCastException(this.Name + " failed to Cast Integer on Instruction.ObjectToProcess");
            }

            // stock Income 
            DebugMessage(" income " + StockElement.Article.Name + " quantity " + stockExchange.Quantity + " added to Stock");
            StockElement.Current += stockExchange.Quantity;
            // change element State to Finish
            stockExchange.State = State.Finished;
            //stockExchange.RequiredOnTime = (int)Context.TimePeriod;
            stockExchange.Time = (int)Context.TimePeriod;

            // no Items to be served.
            if (!RequestedItems.Any()) return;
            
            // Try server all Nonserved Items.
            foreach (var request in RequestedItems.OrderBy(x => x.DueTime).ToList()) // .Where( x => x.DueTime <= Context.TimePeriod))
            {
                var item = StockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == request.StockExchangeId);
                if (item != null) { item.State = State.Finished; item.Time = (int)Context.TimePeriod; }
                else throw new Exception("No StockExchange found");
                CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.RequestProvided.ToString(),
                                                objectToProcess: request,
                                                targetAgent: request.Requester /*, 
                                                    waitFor: request.DueTime */ );
                RequestedItems.Remove(request);
            }
        }

        /// <summary>
        /// Returns the Reservation Amont
        /// TODO: User more Complex Logic
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private StockReservation MakeReservationFor(RequestItem request)
        {
            request.StockExchangeId =  Guid.NewGuid();
            StockReservation stockReservation = new StockReservation { DueTime = request.DueTime };

            var withdrawl = StockElement.StockExchanges
                                .Where(x => x.RequiredOnTime <= request.DueTime &&
                                            x.State != State.Finished &&
                                            x.ExchangeType == ExchangeType.Withdrawal)
                                .Sum(x => x.Quantity);
            // Element is NOT in Stock
            // Create Order if Required.
            var purchaseOpen = StockElement.StockExchanges
                .Any(x => x.State != State.Finished && x.ExchangeType == ExchangeType.Insert);
            var min = ((StockElement.Current - withdrawl - request.Quantity) < StockElement.Min);
            if (min && StockElement.Article.ToPurchase && !purchaseOpen)
            {
                CreatePurchase();
                DebugMessage(" Created purchase for " + this.StockElement.Article.Name);
            }

            //if ((StockElement.Current + insert - withdrawl - request.Quantity) < 0)
            if ((StockElement.Current - withdrawl - request.Quantity) < 0)
            {
                stockReservation.IsInStock = false;
                stockReservation.Quantity = 0;
            }
            else
            {
                stockReservation.IsInStock = true;
                stockReservation.Quantity = request.Quantity;
                StockElement.Current -= request.Quantity;
            }
            
            
            //Create Reservation
            StockElement.StockExchanges.Add(
                new StockExchange
                {
                    TrakingGuid = request.StockExchangeId,
                    StockId = StockElement.Id,
                    ExchangeType = ExchangeType.Withdrawal,
                    Quantity = request.Quantity,
                    Time = (int)Context.TimePeriod,
                    State = stockReservation.IsInStock ? State.Finished : State.Created,
                    RequiredOnTime = request.DueTime,
                }
            );

            return stockReservation;
        }

        private void CreatePurchase()
        {
            var time = StockElement.Article
                                    .ArticleToBusinessPartners
                                    .Single(x => x.BusinessPartner.Kreditor)
                                    .DueTime;
            var stockExchange = new StockExchange
            {
                StockId = StockElement.Id,
                ExchangeType = ExchangeType.Insert,
                State = State.Created,
                Time = (int)Context.TimePeriod,
                Quantity = StockElement.Article.Stock.Max - StockElement.Article.Stock.Min,
                RequiredOnTime = (int)Context.TimePeriod + time
            };

            StockElement.StockExchanges.Add(stockExchange);
            CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.StockRefill.ToString(),
                                    objectToProcess: stockExchange,
                                    targetAgent: this,
                                    // TODO needs logic if more Kreditors are Added.
                                    waitFor: time);


        }

        private void WithdrawlMaterial(InstructionSet instructionSet)
        {
            var request = instructionSet.ObjectToProcess as RequestItem;
            if (request == null)
            {
                throw new InvalidCastException(this.Name + " failed to Cast ProductionAgent on Instruction.ObjectToProcess");
            }
            Withdraw(request);
        }

        private void Withdraw(RequestItem requestItem)
        {
            var item = StockElement.StockExchanges.FirstOrDefault(x => x.TrakingGuid == requestItem.StockExchangeId);
            if (item != null) { item.State = State.Finished; item.Time = (int)Context.TimePeriod; }
            else throw new Exception("No StockExchange found");
        }

    }
}