using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Enums;
using Master40.DB.Models;

namespace Master40.Agents.Agents
{
    public class StorageAgent : Agent
    {
        private Stock StockElement { get; set; }
        public string StockFor { get; }
        private List<RequestItem> RequestedItems { get; set; }

        public StorageAgent(Agent creator, string name, bool debug, Stock stockElement) : base(creator, name, debug)
        {
            StockElement = stockElement;
            StockFor = stockElement.Article.Name;
            RequestedItems = new List<RequestItem>();
            //Instructions = new List<Instruction>{ new Instruction{ Method = "RequestArticle", ExpectedObjecType = typeof(int) } };
        }

        public enum InstuctionsMethods
        {
            // Create and Return a Reservation for Article
            RequestArticle,
            ResponseFromProduction
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
            
            var stockReservation = TryToMakeReservationFor(request);
            RequestedItems.Add(request);

            // Buy if Purchase is True
            if (request.Article.ToPurchase)
            {
                //TODO: Create Purchase maybe with activity 
                // currently just say its in stock.
                stockReservation.IsInStock = true;
                stockReservation.Quantity = request.Quantity;
            }

            // Create Callback // Probably not required here
            CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.ResponseFromStock.ToString(),
                                  objectToProcess: stockReservation, // may needs later a more complex answer for now just remove item from stock
                                      targetAgent: instructionSet.SourceAgent); // its Source Agent becaus this message is the Answer to the Instruction set.
                                     
        }

        private void ResponseFromProduction(InstructionSet instructionSet)
        {
            var productionAgent = instructionSet.ObjectToProcess as ProductionAgent;
            if (productionAgent == null)
            {
                throw new InvalidCastException(this.Name + " failed to Cast ProductionAgent on Instruction.ObjectToProcess");
            }

            DebugMessage("Production Agent Finished Work: " + productionAgent.Name);

            StockElement.Current++;
            var requestProvidable = RequestedItems.FirstOrDefault(x => x.DueTime == RequestedItems.Min(r => r.DueTime));
            if (requestProvidable.Quantity == StockElement.Current)
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
            }
        }



        /// <summary>
        /// Returns the Reservation Amont
        /// TODO: User more Complex Logic
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private StockReservation TryToMakeReservationFor(RequestItem request)
        {

            StockReservation stockReservation = new StockReservation { DueTime = request.DueTime, };

            // Element is NOT in Stock
            if ((StockElement.Current - StockElement.StockExchanges
                                                    .Where(x => x.RequiredOnTime <= request.DueTime)
                                                    .Sum(x => x.Quantity) - request.Quantity) < 0)
            {
                stockReservation.IsInStock = false;
                stockReservation.Quantity = 0;
                return stockReservation;
            }

            // else Create Reservation
            StockElement.StockExchanges.Add(
            new StockExchange
                {
                    StockId = StockElement.Id,
                    EchangeType = EchangeType.Withdrawal,
                    Quantity = request.Quantity,
                    RequiredOnTime = request.DueTime,
                }
            );
            stockReservation.IsInStock = true;
            stockReservation.Quantity = request.Quantity;


            return stockReservation;
        }

        
    }
}