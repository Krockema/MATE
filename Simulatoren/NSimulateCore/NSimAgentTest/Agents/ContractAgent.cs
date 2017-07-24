using System;
using System.Collections.Generic;
using System.Linq;
using NSimAgentTest.Agents.Internal;
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

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param name="objects"></param>
        private void StartOrder(InstructionSet objects)
        {
            var orderItem = objects.ObjectToProcess as RequestItem;
            if (orderItem == null)
                throw new InvalidCastException();

            // Create Dispo Agent 
            var dispoAgent = new DispoAgent(creator: this, 
                                             system: Creator, 
                                               name: orderItem.Name, 
                                              debug: DebugThis, 
                                        requestItem: orderItem);

            ChildAgents.Add(dispoAgent);
        }
    }
}