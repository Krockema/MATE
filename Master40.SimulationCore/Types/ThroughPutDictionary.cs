using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.Types
{
    public class ThroughPutDictionary 
    {
        private Dictionary<string, EstimatedThroughPut> dic = new Dictionary<string, EstimatedThroughPut>();
        public EstimatedThroughPut Get(string name)
        {
            if (dic.TryGetValue(name, out EstimatedThroughPut eta))
            {
                return eta;
            }
            throw new Exception("No Estimated Throughput found");
        }

        public bool UpdateOrCreate(string name, long time)
        {
            if(dic.TryGetValue(name, out EstimatedThroughPut eta))
            {
                eta.Set(time);
                return false;
            }
            // else
            dic.Add(name, new EstimatedThroughPut(time));
            return true;
        }
    }
}
