using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class TimeToAdvance : Option<TimeSpan>
    {
        public TimeToAdvance(TimeSpan value)
        {
            _value = value;
        }

        public TimeToAdvance()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TimeToAdvance(value:TimeSpan.FromMilliseconds(long.Parse(argument))));
            };
        }
    }

}
