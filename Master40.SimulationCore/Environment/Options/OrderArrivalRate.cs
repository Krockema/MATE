using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class OrderArrivalRate : Option<double>
    {
        public static Type Type => typeof(OrderArrivalRate);
        public OrderArrivalRate(double value)
        {
            _value = value;
        }
    }
}
