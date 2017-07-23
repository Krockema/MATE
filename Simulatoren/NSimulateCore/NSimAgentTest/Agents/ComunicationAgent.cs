using System;
using System.Collections.Generic;
using NSimAgentTest.Agents.Internal;
using NSimAgentTest.Enums;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class ComunicationAgent : Agent
    {
        public ComunicationAgent(Agent creator, string name, bool debug, string contractType) 
            : base(creator, name, debug)
        {
            ContractType = contractType;
        }
        public string ContractType { get; set; }
    }
}