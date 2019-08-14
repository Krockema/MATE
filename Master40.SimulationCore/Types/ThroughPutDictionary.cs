using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.SimulationCore.Agents.Types
{
    public class ThroughPutDictionary : Dictionary<string, EstimatedThroughPut>
    {

        public EstimatedThroughPut Get(string name)
        {
            if (this.TryGetValue(name, out EstimatedThroughPut eta))
            {
                return eta;
            }
            throw new Exception("No Estimated Throughput found");
        }

        public bool UpdateOrCreate(string name, long time)
        {
            if(this.TryGetValue(name, out EstimatedThroughPut eta))
            {
                eta.Set(time);
                return false;
            }
            // else
            this.Add(name, new EstimatedThroughPut(time));
            return true;
        }
    }
}
