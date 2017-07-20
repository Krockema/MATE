using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using NSimAgentTest.Enums;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public abstract class Agent : Process
    {
        Guid AgentId { get; set; }
        Status Status { get; set; }
        public abstract void Destroy();
        public abstract override IEnumerator<InstructionBase> Simulate();
    }
}