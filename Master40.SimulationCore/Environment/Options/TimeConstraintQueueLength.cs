using Master40.SimulationCore.Environment.Abstractions;
using System;


namespace Master40.SimulationCore.Environment.Options
{
    public class TimeConstraintQueueLength : Option<int>
    {
            public static Type Type => typeof(TimeConstraintQueueLength);
            public TimeConstraintQueueLength(int value)
            {
                _value = value;
        }
    }
}


