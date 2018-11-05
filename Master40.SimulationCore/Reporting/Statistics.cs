using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Reporting
{
    public class Statistics
    {
        public string AgentId { get; set; }
        public string AgentType { get; set; }
        public string AgentName { get; set; }
        public long VirtualTime { get; set; }
        public long ProcessingTime { get; set; }
        public static List<string> Log { get; set; }
    }
}
