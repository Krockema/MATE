using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Data.Helper;
using Master40.DB.Models;

namespace Master40.Agents.Agents
{
    public class DispoAgent : Agent
    {
        public Agent SystemAgent { get; }
        private RequestItem RequestItem { get; set; }
        private int QuantityToProduce { get; set; }
        private Agent StockAgent { get; set; }
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
        public DispoAgent(Agent creator, Agent system, string name, bool debug, RequestItem requestItem) : base(creator, name, true)//debug)
        {
            SystemAgent = system;
            RequestItem = requestItem;
            RequestItem.Requester = this;
            //Instructions.Add(new Instruction { Method = "ResponseFromStock", ExpectedObjecType = typeof(string) });
            //Instructions.Add(new Instruction { Method = "CreateProductionAgent", ExpectedObjecType = typeof(string) });
            RequestFromStock();
        }

        public enum InstuctionsMethods
        {
            ResponseFromStock,
            RequestProvided,
            ResponseFromSystemForBom
        }

        private void RequestFromStock()
        {
            // get Related Storage Agent
            // first catch the correct Storage Agent
            // TODO Could be managent by Dictonary like Machine <-> Dictionary <-> Comunication
            StockAgent = SystemAgent.ChildAgents.OfType<StorageAgent>()
                                        .FirstOrDefault(x => x.StockFor == RequestItem.Article.Name);
            if (StockAgent == null)
            {
                throw new ArgumentNullException($"No storage agent found for {1}", RequestItem.Article.Name);
            }
            // debug
            DebugMessage("requests from " + StockAgent.Name + " Article: " + RequestItem.Article.Name + " (" + RequestItem.Quantity + ")");
            
            // Create Request 
            CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.RequestArticle.ToString(),
                                  objectToProcess: RequestItem,
                                      targetAgent: StockAgent);
        }


        private void ResponseFromStock(InstructionSet instructionSet)
        {
            if(instructionSet.ObjectToProcess != null)
            {
                QuantityToProduce = RequestItem.Quantity - ((StockReservation)instructionSet.ObjectToProcess).Quantity;
            }
            // TODO -> Logic
            DebugMessage("Returned with " + ((StockReservation)instructionSet.ObjectToProcess).Quantity + " " + RequestItem.Article.Name + " Reserved!");

            
            // Create Production Agents
            if (RequestItem.Article.ToBuild)
            {
                CreateAndEnqueueInstuction(methodName: Agents.SystemAgent.InstuctionsMethods.RequestArticleBom.ToString(),
                                        objectToProcess: RequestItem,
                                        targetAgent: SystemAgent);
            }

            // check for zero
            if (QuantityToProduce == 0) this.Finish();
        }

        private void ResponseFromSystemForBom(InstructionSet instructionSet)
        {
            var article = instructionSet.ObjectToProcess as Article;
            if (article == null)
            {
                throw new InvalidCastException(this.Name + " failed to Cast Article on Instruction.ObjectToProcess");
            }

            // create new Request Item
            RequestItem item = RequestItem.CopyProperties();
            item.Article = article;
            item.OrderId = RequestItem.OrderId;


            // set new Due Time if there is Work to do.
            if (RequestItem.Article.WorkSchedules != null)
                item.DueTime = RequestItem.DueTime - RequestItem.Article.WorkSchedules.Sum(x => x.Duration);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < QuantityToProduce; i++)
            {
                ChildAgents.Add(new ProductionAgent(creator: StockAgent,
                                                       name: "Production(" + RequestItem.Article.Name + ", Nr. " + i + ")",
                                                      debug: DebugThis,
                                                requestItem: item));
            }
        }

        private void RequestProvided(InstructionSet instructionSet)
        {
            this.Finish();
        }
    }
}