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

        private void RequestArticle(InstructionSet instructionSet)
        {
            if (DebugThis)
            {
                Console.WriteLine(this.Name +  ": Request Article " + StockElement.Name + " from " + instructionSet.SourceAgent.Name + "");
            }

            var numberOfReservedItems =  TryToMakeReservationFor(article: instructionSet.ObjectToProcess as RequestItem);

            instructionSet.SourceAgent.InstructionQueue.Enqueue(
                new InstructionSet
                {
                    MethodName = DispoAgent.InstuctionsMethods.ResponseFromStock.ToString(),
                    ObjectToProcess = numberOfReservedItems, // may needs later a more complex answer for now just remove item from stock
                    ObjectType = typeof(int),
                    SourceAgent = this,
                });
            Context.ProcessesRemainingThisTimePeriod.Enqueue(instructionSet.SourceAgent);
        }

        /// <summary>
        /// Returns the Reservation Amont
        /// TODO: User more Complex Logic
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        private int TryToMakeReservationFor(RequestItem article)
        {
            if ((StockElement.Quantity - article.Quantity) <= 0) return 0;
            StockElement.Quantity = StockElement.Quantity - article.Quantity;
            return article.Quantity;
        }

    }
}