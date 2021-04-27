using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class MinDeliveryTime : Option<int>
    {
        public MinDeliveryTime(int value)
        {
            _value = value;
        }

        public MinDeliveryTime()
        {
            Action = (config, argument) => {
                config.AddOption(o: new MinDeliveryTime(value: int.Parse(s: argument)));
            };
        }
    }
}
