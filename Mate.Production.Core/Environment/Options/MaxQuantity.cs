using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class MaxQuantity : Option<int>
    {
        public MaxQuantity(int value)
        {
            _value = value;
        }

        public MaxQuantity()
        {
            Action = (config, argument) => {
                config.AddOption(o: new MaxQuantity(value: int.Parse(s: argument)));
            };
        }
    }
}
