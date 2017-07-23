using System;
using System.Collections.Generic;
using NSimAgentTest.Agents.Internal;
using NSimAgentTest.Enums;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class ProductionAgent : Agent
    {
        public ProductionAgent(Agent creator, string name, bool debug) : base(creator, name , debug) { }
    }
}