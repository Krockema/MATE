using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class SimulationNumber : Option<int>
    {
        public static Type Type => typeof(SimulationNumber);
        public SimulationNumber(int value)
        {
            _value = value;
        }
    }
}
