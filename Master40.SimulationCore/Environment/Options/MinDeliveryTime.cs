using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class MinDeliveryTime : Option<int>
    {
        public static Type Type => typeof(MinDeliveryTime);
        public MinDeliveryTime(int value)
        {
            _value = value;
        }
    }
}
