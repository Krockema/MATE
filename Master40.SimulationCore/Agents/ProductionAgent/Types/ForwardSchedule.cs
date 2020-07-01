using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.ProductionAgent.Types
{
    public class ForwardSchedule
    {
        public object Child { get; set; }
        public long LatestEnd { get; set; }
    }
}
