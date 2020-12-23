using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class UsePredictedThroughput : Option<bool>
    {
        public static Type Type => typeof(UsePredictedThroughput);
        public UsePredictedThroughput(bool value)
        {
            _value = value;
        }
    }
}