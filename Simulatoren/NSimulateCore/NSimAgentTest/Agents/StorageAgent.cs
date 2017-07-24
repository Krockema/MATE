using System;
using System.Collections.Generic;
using NSimAgentTest.Agents.Internal;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class StorageAgent : Agent
    {
        public StockElement StockElement { get; set; }
        public StorageAgent(Agent creator, string name, bool debug, StockElement stockElement) : base(creator, name, debug)
        {
            StockElement = stockElement;
        }

        public enum InstuctionsMethods
        {
            // Create and Return a Reservation for Article
            RequestArticle,
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
            var numberOfReservedItems =  TryToMakeReservationFor(request: request);
            
            // Create Callback
            CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.ResponseFromStock.ToString(),
                                  objectToProcess: numberOfReservedItems, // may needs later a more complex answer for now just remove item from stock
                                      targetAgent: instructionSet.SourceAgent, // its Source Agent becaus this message is the Answer to the Instruction set.
                                      sourceAgent: this);
        }

        /// <summary>
        /// Returns the Reservation Amont
        /// TODO: User more Complex Logic
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private int TryToMakeReservationFor(RequestItem request)
        {
            if ((StockElement.Quantity - request.Quantity) < 0) return 0;
            StockElement.Quantity = StockElement.Quantity - request.Quantity;
            return request.Quantity;
        }

    }
}