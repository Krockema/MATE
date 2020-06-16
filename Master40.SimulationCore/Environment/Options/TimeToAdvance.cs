using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class TimeToAdvance : Option<TimeSpan>
    {
        public static Type Type => typeof(TimeToAdvance);
        public TimeToAdvance(TimeSpan value)
        {
            _value = value;
        }
    }

}
