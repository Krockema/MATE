using System;
using System.Diagnostics;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Models;
using NSimulate.Instruction;

namespace Master40.Agents.Agents
{
    public class ContractAgent : Agent
    {
        private RequestItem requestItem;

        public ContractAgent(Agent creator, string name, bool debug) : base(creator, name, debug)
        {
            //Instructions.Add(new Instruction{ Method = "StartOrder", ExpectedObjecType = typeof(RequestItem) });
        }



        public enum InstuctionsMethods
        {
            // Create and Return a Reservation for Article
            StartOrder,
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param name="objects"></param>
        private void StartOrder(InstructionSet objects)
        {
            var orderItem = objects.ObjectToProcess as OrderPart;
            if (orderItem == null)
                throw new InvalidCastException();

            // create Request Item
            requestItem = MapPropertiesToRequestItem(orderItem);

            // Create Dispo Agent 
            var dispoAgent = new DispoAgent(creator: this, 
                                             system: Creator, 
                                               name: requestItem.Article.Name + " OrderPartId(" + orderItem.Id + ")", 
                                              debug: DebugThis, 
                                        requestItem: requestItem);

            ChildAgents.Add(dispoAgent);
        }

        private RequestItem MapPropertiesToRequestItem(OrderPart orderPart)
        {
            return new RequestItem
            {
                DueTime = orderPart.Order.DueTime,
                Quantity = orderPart.Quantity,
                Article = orderPart.Article,
                OrderId = orderPart.Id,
                IsHeadDemand = true
               // IDemandToProvider = orderPart.DemandOrderParts.FirstOrDefault()
            };
        }

        internal new void Finished(InstructionSet instructionSet)
        {
            if (ChildAgents.Any(x => x.Status != Status.Finished)) return;
            // else
            // Call Finish
            var intime = false;
            if (Context.TimePeriod <= requestItem.DueTime)
                intime = true;
            Debug.WriteLine("Order Finished at:" + Context.TimePeriod + " InTime: " + intime);
            CreateAndEnqueueInstuction(methodName: SystemAgent.InstuctionsMethods.OrderProvided.ToString(),
                objectToProcess: requestItem,
                targetAgent: this.Creator);
        }




    }
}