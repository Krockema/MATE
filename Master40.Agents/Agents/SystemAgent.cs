using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.Agents.Agents
{
    public class SystemAgent : Agent
    {
        private readonly ProductionDomainContext _productionDomainContext;

        public SystemAgent(Agent creator, string name, bool debug, ProductionDomainContext productionDomainContext) : base(creator, name, debug)
        {
            this._productionDomainContext = productionDomainContext;
        }
        public enum InstuctionsMethods
        {
            CreateContractAgent,
            RequestProductionOrder,
        }
        //TODO: System Talk.

        private void CreateContractAgent(InstructionSet instructionSet)
        {
            //  Check 0 ref
            var requestItem = instructionSet.ObjectToProcess as OrderPart;
            if (requestItem == null)
            {
                throw new InvalidCastException(this.Name + " Cast to OrderPart Failed");
            }

            // Create DemandRequester
            var demand = _productionDomainContext.CreateDemandOrderPart(requestItem);
            requestItem.DemandOrderParts = new List<DemandOrderPart>{ (DemandOrderPart)demand };

            // Create Agent
            CreateAgents(requestItem);
        }

        private void RequestProductionOrder(InstructionSet instructionSet)
        {
            //  Check 0 ref
            var requestItem = instructionSet.ObjectToProcess as RequestItem;
            if (requestItem == null)
            {
                throw new InvalidCastException(this.Name + " Cast to RequestItem Failed");
            }

            // debug
            DebugMessage(" Request BOM for Production Order" + requestItem.Article.Name);

            // get BOM from Context
            var productionOrder =
                _productionDomainContext.CopyArticleToProductionOrder(requestItem.Article.Id
                                                                    , requestItem.Quantity
                                                                    , requestItem.IDemandToProvider.Id);

            // calback with po.bom
            CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.ResponseFromSystemForBOM.ToString(),
                                  objectToProcess: productionOrder,
                                      targetAgent: instructionSet.SourceAgent);

        }


        private void CreateAgents(OrderPart contract)
        {
            // Create Contract agents
            var ca = new ContractAgent(creator: this,
                name: contract.Order.Name + " - Part:" + contract.Article.Name,
                debug: true);
            // add To System
            this.ChildAgents.Add(ca);

            // enqueue Order
            CreateAndEnqueueInstuction(methodName: ContractAgent.InstuctionsMethods.StartOrder.ToString(),
                                  objectToProcess: contract,
                                      targetAgent: ca);

        }

        public void PrepareAgents()
        {
            foreach (var orderpart in _productionDomainContext.OrderParts
                                                                .Include(x => x.Article)
                                                                .Include(x => x.Order)
                                                                .AsNoTracking())
            {
                this.InstructionQueue.Enqueue(new InstructionSet
                {
                    MethodName = SystemAgent.InstuctionsMethods.CreateContractAgent.ToString(),
                    ObjectToProcess = orderpart,
                    ObjectType = orderpart.GetType(),
                    SourceAgent = this,
                });
            }
        }

    }
}