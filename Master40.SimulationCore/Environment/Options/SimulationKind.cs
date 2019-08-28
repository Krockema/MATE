using Master40.DB.Enums;
using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class SimulationKind : Option<SimulationType>
    {
        public static Type Type => typeof(SimulationKind);
        public SimulationKind(SimulationType value)
        {
            _value = value;
        }
    }
}
