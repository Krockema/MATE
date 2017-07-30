using System.Collections.Generic;
using NSimulate;
using NSimulate.Instruction;

namespace Master40.Agents.Agents.Internal
{
    public class AgentActivity : Process
    {
        private InstructionSet InstructionSet;
        private Agent TargetAgent;
        private long WaitFor;
        
        public AgentActivity(InstructionSet instructionSet , Agent targetAgent, long waitFor)
        //public AgentActivity(Agent targetAgent, long waitFor)
        {
            InstructionSet = instructionSet;
            TargetAgent = targetAgent;
            WaitFor = waitFor;

        }

        public override IEnumerator<InstructionBase> Simulate()
        {
            // wait for the reorder time appropriate for the product
            
            yield return new WaitInstruction(WaitFor);
            TargetAgent.InstructionQueue.Enqueue(InstructionSet);
            Context.ProcessesRemainingThisTimePeriod.Enqueue(TargetAgent);

        }
    }
}