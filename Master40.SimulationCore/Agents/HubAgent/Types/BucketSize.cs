using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class BucketSize
    {
        public M_ResourceCapability Capability { get; set; }
        public long Duration { get; set; }
        public long Size { get; set; }
        public double Ratio { get; set; }
    }
}
