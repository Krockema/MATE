using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class SettlingStart : Option<int>
    {
        public static Type Type => typeof(SettlingStart);
        public SettlingStart(int value)
        {
            _value = value;
        }
    }
}
