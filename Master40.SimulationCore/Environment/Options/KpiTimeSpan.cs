using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class KpiTimeSpan : Option<long>
    {
        public static Type Type => typeof(KpiTimeSpan);
        public KpiTimeSpan(long value)
        {
            _value = value;
        }
    }
}
