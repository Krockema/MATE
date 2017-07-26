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
        private RequestItem RequestItem { get; set; }

        /// <summary>
        /// First  ask Store for Item and wait for Response
        /// secont Create Production
        /// Third  Call Contract and Store that order has been Reserved / Produced
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="system"></param>
        /// <param name="name"></param>
        /// <param name="debug"></param>
        /// <param name="requestItem"></param>
        public DispoAgent(Agent creator, Agent system, string name, bool debug, RequestItem requestItem) : base(creator, name, debug)
        {
            _system = system;
            RequestItem = requestItem;
            //Instructions.Add(new Instruction { Method = "ResponseFromStock", ExpectedObjecType = typeof(string) });
            //Instructions.Add(new Instruction { Method = "CreateProductionAgent", ExpectedObjecType = typeof(string) });

            RequestFromStock();
        }

        public enum InstuctionsMethods
        {
            ResponseFromStock,
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
            var quantityToProduce = 0;
            if(instructionSet.ObjectToProcess != null)
            {
                quantityToProduce = RequestItem.Quantity - (int) instructionSet.ObjectToProcess;
            }
            // TODO -> Logic
            DebugMessage(" Returned with " + instructionSet.ObjectToProcess + " Items Reserved!");

            // check for zero
            if (quantityToProduce == 0)
            {
                this.Finish();
            }

            // Create Request Productionorder
            CreateAndEnqueueInstuction(methodName: SystemAgent.InstuctionsMethods.RequestProductionOrder.ToString(),
                                  objectToProcess: RequestItem,
                                      targetAgent: _system,
                                      sourceAgent: this);
        }

        private void ResponseForBOM(InstructionSet instructionSet)
        {

            ChildAgents.Add(new ProductionAgent(this, "Production(" + RequestItem.Name + ")", DebugThis));


            CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
                objectToProcess: RequestItem,
                targetAgent: _system,
                sourceAgent: this);

        }

   

        
    }
}