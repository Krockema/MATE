using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class UsePredictedThroughput : Option<int>
    {
        public static Type Type => typeof(UsePredictedThroughput);
        public UsePredictedThroughput(int value)
        {
            _value = value;
        }
    }
}