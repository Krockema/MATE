using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class Seed : Option<int>
    {
        public static Type Type => typeof(Seed);
        public Seed(int value)
        {
            _value = value;
        }
    }
}
