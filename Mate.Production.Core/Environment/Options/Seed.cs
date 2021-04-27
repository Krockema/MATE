using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class Seed : Option<int>
    {
        public Seed(int value)
        {
            _value = value;
        }

        public Seed()
        {
            Action = (config, argument) => {
                config.AddOption(o: new Seed(value: int.Parse(s: argument)));
            };
        }
    }
}
