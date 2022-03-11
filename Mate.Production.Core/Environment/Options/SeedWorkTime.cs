using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SeedWorkTime : Option<int>
    {
        public SeedWorkTime(int value)
        {
            _value = value;
        }

        public SeedWorkTime()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SeedWorkTime(value: int.Parse(s: argument)));
            };
        }
    }
}
