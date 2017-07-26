using System;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Master40.Agents.Agents
{
    public class SystemAgent : Agent
    {
        private readonly ProductionDomainContext _productionDomainContext;

        public SystemAgent(Agent creator, string name, bool debug, ProductionDomainContext _productionDomainContext) : base(creator, name, debug)
        {
            this._productionDomainContext = _productionDomainContext;
        }
        public enum InstuctionsMethods
        {
            RequestProductionOrder,
        }
        //TODO: System Talk.

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
                _productionDomainContext.CopyArticleToProductionOrder(requestItem.Article.Id, requestItem.Quantity);

            // calback with po.bom
            CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.ResponseFromSystemForBOM.ToString(),
                                  objectToProcess: productionOrder,
                                      targetAgent: instructionSet.SourceAgent);

        }

    }
}