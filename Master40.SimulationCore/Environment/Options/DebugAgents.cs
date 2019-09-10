using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class DebugAgents : Option<bool>
    {
        public static Type Type => typeof(DebugAgents);
        public DebugAgents(bool value)
        {
            _value = value;
        }
    }
}
