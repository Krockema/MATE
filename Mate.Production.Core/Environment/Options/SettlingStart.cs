using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SettlingStart : Option<long>
    {
        public SettlingStart(long value)
        {
            _value = value;
        }

        public SettlingStart()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SettlingStart(value: long.Parse(s: argument)));
            };
        }
    }
}
