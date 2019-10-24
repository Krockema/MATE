using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.CollectorAgent.Types
{
    public class ResourceSimulationData
    {
        internal string _resource { get; set; }
        internal string _setupTime { get; set; }
        internal string _workTime { get; set; }

        public ResourceSimulationData(string resource)
        {
            _resource = resource;
        }
    }
}
