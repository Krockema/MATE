using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class OrderArrivalRate : Option<double>
    {
        public OrderArrivalRate(double value)
        {
            _value = value;
        }

        public OrderArrivalRate()
        {
            Action = (config, argument) => {
                config.AddOption(o: new OrderArrivalRate(value: double.Parse(s: argument)));
            };
        }
    }
}
