using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class OrderQuantity : Option<int>
    {
        public static Type Type => typeof(OrderQuantity);
        public OrderQuantity(int value)
        {
            _value = value;
        }
    }
}
