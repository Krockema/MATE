using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class WorkTimeDeviation : Option<double>
    {
        public static Type Type => typeof(WorkTimeDeviation);
        public WorkTimeDeviation(double value)
        {
            _value = value;
        }
    }
}
