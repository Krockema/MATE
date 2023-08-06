using Mate.Production.Core.Environment.Abstractions;
using System;

namespace Mate.Production.Core.Environment.Options
{
    public class TimeConstraintQueueLength : Option<TimeSpan>
    {
        public TimeConstraintQueueLength(TimeSpan value)
        {
            _value = value;
        }

        public TimeConstraintQueueLength()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TimeConstraintQueueLength(value: TimeSpan.Parse(s: argument)));
            };
        }
    }
}


