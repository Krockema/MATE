using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class MinQuantity : Option<int>
    {
        public MinQuantity(int value)
        {
            _value = value;
        }

        public MinQuantity()
        {
            Action = (config, argument) => {
                config.AddOption(o: new MinQuantity(value: int.Parse(s: argument)));
            };
        }
    }
}
