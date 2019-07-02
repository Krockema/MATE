using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class SimulationId : Option<int>
    {
        public static Type Type => typeof(SimulationId);
        public SimulationId(int value)
        {
            _value = value;
        }
    }
}
