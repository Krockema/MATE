using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class OrderQuantity : Option<int>
    {
        public OrderQuantity(int value)
        {
            _value = value;
        }

        public OrderQuantity()
        {
            Action = (config, argument) => {
                config.AddOption(o: new OrderQuantity(value: int.Parse(s: argument)));
            };
        }
    }
}
