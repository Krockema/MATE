using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class EstimatedThroughPut : Option<long>
    {
        public static Type Type = typeof(EstimatedThroughPut);
        public EstimatedThroughPut(long value)
        {
            _value = value;
        }

        public void Set(long time)
        {
            _value = time;
        }
    }
}
