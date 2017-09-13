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
            //Instructions = new List<Instruction>{ new Instruction{ Method = "RequestArticle", ExpectedObjecType = typeof(int) } };
        }

        public enum InstuctionsMethods
        {
            // Create and Return a Reservation for Article
            RequestArticle,
            ResponseFromProduction,
            StockRefill
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
            StockElement.StockExchanges.Add(
                new StockExchange
                {
                    StockId = StockElement.Id,
                    ExchangeType = ExchangeType.Insert,
                    Quantity = 1,
                    RequiredOnTime = (int)Context.TimePeriod
                });

            ProviderList.Add(productionAgent.AgentId);
            // Check if the most Important Request can be provided.
            var requestProvidable = RequestedItems.FirstOrDefault(x => x.DueTime == RequestedItems.Min(r => r.DueTime));
            if (requestProvidable.Quantity <= StockElement.Current)
            {
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
                Statistics.UpdateSimulationWorkSchedule(ProviderList, requestProvidable.Requester.Creator, requestProvidable.OrderId);
                ProviderList.Clear();
            }
    }

        private void StockRefill(InstructionSet instructionSet)
        {
            var quantity = instructionSet.ObjectToProcess is decimal;
            if (!quantity)
            {
                throw new InvalidCastException(this.Name + " failed to Cast Integer on Instruction.ObjectToProcess");
            }

            // stock Income 
            DebugMessage(" income " + StockElement.Article.Name + " quantity " + (decimal)instructionSet.ObjectToProcess + " added to Stock");
            StockElement.Current += (decimal)instructionSet.ObjectToProcess;


            // no Items to be served.
            if (!RequestedItems.Any()) return;
            
            // Try server all Nonserved Items.
            foreach (var request in RequestedItems.OrderBy(x => x.DueTime).ToList()) // .Where( x => x.DueTime <= Context.TimePeriod))
            {
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

            StockReservation stockReservation = new StockReservation { DueTime = request.DueTime, };

            // Element is NOT in Stock
            if ((StockElement.Current - StockElement.StockExchanges
                     .Where(x => x.RequiredOnTime <= request.DueTime)
                     .Sum(x => x.Quantity) - request.Quantity) < 0)
            {
                stockReservation.IsInStock = false;
                stockReservation.Quantity = 0;
                // Create Order 
                if (!stockReservation.IsInStock && StockElement.Article.ToPurchase)
                {
                    CreatePurchase();
                    DebugMessage(" Created purchase for " + this.StockElement.Article.Name);
                }
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
                    StockId = StockElement.Id,
                    ExchangeType = ExchangeType.Withdrawal,
                    Quantity = request.Quantity,
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

            CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.StockRefill.ToString(),
                                    objectToProcess: StockElement.Article.Stock.Max,
                                    targetAgent: this,
                                    // TODO needs logic if more Kreditors are Added.
                                    waitFor: time);

            StockElement.StockExchanges.Add(
                new StockExchange
                {
                    StockId = StockElement.Id,
                    ExchangeType = ExchangeType.Insert,
                    Quantity = StockElement.Article.Stock.Max,
                    RequiredOnTime = time,
                }
            );
        }

    }
}