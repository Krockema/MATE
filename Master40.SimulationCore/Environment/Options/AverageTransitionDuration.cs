using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class TransitionFactor : Option<decimal>
    {
        public static Type Type => typeof(TransitionFactor);
        public TransitionFactor(decimal value)
        {
            _value = value;
        }
    }
}
