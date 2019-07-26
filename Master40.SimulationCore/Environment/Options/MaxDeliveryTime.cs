using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class MaxDeliveryTime : Option<int>
    {
        public static Type Type => typeof(MaxDeliveryTime);
        public MaxDeliveryTime(int value)
        {
            _value = value;
        }
    }
}
