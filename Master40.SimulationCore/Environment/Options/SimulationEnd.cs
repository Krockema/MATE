using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class SimulationEnd : Option<long>
    {
        public static Type Type = typeof(SimulationEnd);
        public SimulationEnd(long value)
        {
            _value = value;
        }
    }
}
