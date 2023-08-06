using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SettlingStart : Option<TimeSpan>
    {
        public SettlingStart(TimeSpan value)
        {
            _value = value;
        }

        public SettlingStart()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SettlingStart(value: TimeSpan.Parse(s: argument)));
            };
        }
    }
}
