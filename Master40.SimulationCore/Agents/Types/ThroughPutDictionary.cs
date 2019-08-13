using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.SimulationCore.Agents.Types
{
    public class ThroughPutDictionary : Dictionary<string, EstimatedThroughPut>
    {

        public EstimatedThroughPut GetThroughPut(string name)
        {
            if (this.TryGetValue(name, out EstimatedThroughPut eta))
            {
                return eta;
            }
            throw new Exception("No Estimated Throughput found");
        }
    }
}
