using System.Collections.Generic;
using Master40.SimulationCore.Environment.Options;

namespace Master40.SimulationCore.Types
{
    public class ThroughPutDictionary 
    {
        private Dictionary<string, EstimatedThroughPut> dic = new Dictionary<string, EstimatedThroughPut>();

        /// <summary>
        /// If no throughput for article exists return fix time, TODO forward scheduling
        /// </summary>
        /// <param name="name">Name of the Article</param>
        /// <returns></returns>
        public EstimatedThroughPut Get(string name)
        {
            if (dic.TryGetValue(key: name, value: out EstimatedThroughPut eta))
            {
                return eta;
            }
            return new EstimatedThroughPut(value: 0);
        }

        public bool UpdateOrCreate(string name, long time)
        {
            if(dic.TryGetValue(key: name, value: out EstimatedThroughPut eta))
            {
                eta.Set(time: time);
                return false;
            }
            // else
            dic.Add(key: name, value: new EstimatedThroughPut(value: time));
            return true;
        }
    }
}
