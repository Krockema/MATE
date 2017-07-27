using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Models;

namespace Master40.Agents.Agents
{
    public class DispoAgent : Agent
    {
        private Agent _system;
        private RequestItem RequestItem { get; set; }
        private int quantityToProduce { get; set; }

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
            ResponseFromSystemForBOM
        }

        private void RequestFromStock()
        {
            // get Related Storage Agent
            // first catch the correct Storage Agent
            // TODO Could be managent by Dictonary like Machine <-> Dictionary <-> Comunication
            var storeAgent = _system.ChildAgents.OfType<StorageAgent>().FirstOrDefault(x => x.StockElement.Article.Name == RequestItem.Article.Name);
            if (storeAgent == null)
            {
                throw new ArgumentNullException();
            }
            
            // debug
            DebugMessage("requests from " + storeAgent.Name + " Article: " + RequestItem.Article.Name + " (" + RequestItem.Quantity + ")");
            
            // Create Request 
            CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
                                  objectToProcess: RequestItem,
                                      targetAgent: storeAgent);
        }


        private void ResponseFromStock(InstructionSet instructionSet)
        {
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
                return;
            }

            // Create Request Productionorder
            CreateAndEnqueueInstuction(methodName: SystemAgent.InstuctionsMethods.RequestProductionOrder.ToString(),
                                  objectToProcess: RequestItem,
                                      targetAgent: _system);
        }

        private void ResponseFromSystemForBOM(InstructionSet instructionSet)
        {
            var productionOrders = instructionSet.ObjectToProcess as ProductionOrder;
            if (productionOrders == null)
            {
                throw new InvalidCastException(this.Name + " failed to Cast ProductionOrder on Instruction.ObjectToProcess");
            }

            for (int i = 0; i < quantityToProduce; i++)
            {
                ChildAgents.Add(new ProductionAgent(creator: this, 
                                                       name: "Production(" + RequestItem.Article.Name + ")", 
                                                      debug: DebugThis, 
                                            productionOrder: productionOrders));
            }


            //CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
            //                      objectToProcess: RequestItem,
            //                          targetAgent: _system);

        }

   

        
    }
}