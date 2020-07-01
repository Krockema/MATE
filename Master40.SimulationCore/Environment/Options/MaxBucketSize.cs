using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class MaxBucketSize : Option<long>
    {
        public static Type Type => typeof(MaxBucketSize);
        public MaxBucketSize(long value)
        {
            _value = value;
        }
    }
}