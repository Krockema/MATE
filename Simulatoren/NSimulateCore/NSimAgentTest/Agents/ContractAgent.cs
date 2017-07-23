using System;
using System.Collections.Generic;
using System.Linq;
using NSimAgentTest.Agents.Internal;
using NSimAgentTest.Enums;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class ContractAgent : Agent
    {
        public ContractAgent(Agent creator, string name, bool debug) : base(creator, name , debug) { }

        public enum InstuctionsMethods
        {
            StartOrder
        }


        private void StartOrder(InstructionSet objects)
        {
            var orderItem = objects.ObjectToProcess as RequestItem;
            if (orderItem == null)
            {
                throw new InvalidCastException();
            }

            var dispoAgent = new DispoAgent(creator: this, name: orderItem.Name, debug: this.DebugThis, requestItem: orderItem);
            Context.Register(typeToRegister: typeof(DispoAgent) , objectToRegister: dispoAgent);
            ChildAgents.Add(dispoAgent);
        }
    }
}