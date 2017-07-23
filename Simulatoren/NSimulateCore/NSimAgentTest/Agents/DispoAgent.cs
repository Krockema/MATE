using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NSimAgentTest.Agents.Internal;
using NSimAgentTest.Enums;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class DispoAgent : Agent
    {
        public RequestItem RequestItem { get; set; }
        public int Quantity { get; set; }

        public DispoAgent(Agent creator, string name, bool debug, RequestItem requestItem) : base(creator, name, debug)
        {
            RequestItem = requestItem;
            RequestFromStock();
        }
        public enum InstuctionsMethods
        {
            // First ask Store for Item and wait for Response
            ResponseFromStock,
            // secont Create Production
            CreateProductionAgent,
            // Call Contract and Store that order has been Reserved / Produced
            Finish
        }

        private void RequestFromStock()
        {
            // get Related Storage Agent
            // first Creator:Contract -> secont Creator:System
            var storeAgent = this.Creator.Creator.ChildAgents.OfType<StorageAgent>().FirstOrDefault(x => x.StockElement.Name == RequestItem.Article);
            if (storeAgent == null)
            {
                throw new ArgumentNullException();
            }
            
            // debug
            if (DebugThis)
            {
                Console.WriteLine(this.Name + " requests from " + storeAgent.Name + " Article: " + RequestItem.Name + ":" + RequestItem.Quantity);
            }

            // Create Request 
            storeAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
                ObjectToProcess = RequestItem,
                ObjectType = typeof(RequestItem),
                SourceAgent = this
            });
            Context.ProcessesRemainingThisTimePeriod.Enqueue(storeAgent);
        }

        private void ResponseFromStock(InstructionSet instructionSet)
        {
            if (DebugThis)
            {
                Console.WriteLine(Name  + " Returned with " + instructionSet.ObjectToProcess.ToString() + "Items Reserved!");
            }
        }





    }
}