using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NSimAgentTest.Agents.Internal;
using NSimulate;

namespace NSimAgentTest.Agents
{
    public class SystemAgent : Agent
    {
        private readonly DBContext _dbContext;

        public SystemAgent(Agent creator, string name, bool debug, DBContext _dbContext) : base(creator, name, debug)
        {
            this._dbContext = _dbContext;
        }
        public enum InstuctionsMethods
        {
            RequestProductionOrder,
        }
        //TODO: System Talk.

        private void RequestProductionOrder(InstructionSet instructionSet)
        {
            var processObject = instructionSet.ObjectToProcess as RequestItem;
            if (processObject == null)
            {
                throw new InvalidCastException(this.Name + "Cast to RequestItem Failed");
            }

            DebugMessage("Request for Production Order" + instructionSet.ObjectToProcess);

            var item = this._dbContext.ProductionOrder.SingleOrDefault(x => x.Name == processObject.Article);

        }

    }
}