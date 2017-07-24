using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NSimAgentTest.Agents.Internal;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class DispoAgent : Agent
    {
        private Agent _system;
        public RequestItem RequestItem { get; set; }
        public int Quantity { get; set; }

        public DispoAgent(Agent creator, Agent system, string name, bool debug, RequestItem requestItem) : base(creator, name, debug)
        {
            _system = system;
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
            // first catch the correct Storage Agent
            // TODO Could be managent by Dictonary like Machine <-> Dictionary <-> Comunication
            var storeAgent = _system.ChildAgents.OfType<StorageAgent>().FirstOrDefault(x => x.StockElement.Name == RequestItem.Article);
            if (storeAgent == null)
            {
                throw new ArgumentNullException();
            }
            
            // debug
            DebugMessage(" requests from " + storeAgent.Name + " Article: " + RequestItem.Name + " (" + RequestItem.Quantity + ")");
            
            // Create Request 
            CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
                                  objectToProcess: RequestItem,
                                      targetAgent: storeAgent,
                                      sourceAgent: this);
        }


        private void ResponseFromStock(InstructionSet instructionSet)
        {
            // TODO -> Logic
            DebugMessage(" Returned with " + instructionSet.ObjectToProcess + " Items Reserved!");
        }





    }
}