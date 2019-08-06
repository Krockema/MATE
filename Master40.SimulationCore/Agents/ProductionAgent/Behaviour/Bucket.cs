using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public class Bucket : Default
    {
        internal Bucket(Dictionary<string, object> properties) : base(properties) { }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                
                // case Instruction.Finished f: agent.TryToFinish(f.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }


    }
}
