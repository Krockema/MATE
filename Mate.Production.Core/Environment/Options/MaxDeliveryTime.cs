using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class MaxDeliveryTime : Option<int>
    {
        public MaxDeliveryTime(int value)
        {
            _value = value;
        }

        public MaxDeliveryTime()
        {
            Action = (config, argument) => {
                config.AddOption(o: new MaxDeliveryTime(value: int.Parse(s: argument)));
            };
        }
    }
}
