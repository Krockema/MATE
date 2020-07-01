using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class DebugSystem : Option<bool>
    {
        public static Type Type => typeof(DebugSystem);
        public DebugSystem(bool value)
        {
            _value = value;
        }
    }
}
