using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class TimeConstraintQueueLength : Option<long>
    {
        public TimeConstraintQueueLength(long value)
        {
            _value = value;
        }

        public TimeConstraintQueueLength()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TimeConstraintQueueLength(value: long.Parse(s: argument)));
            };
        }
    }
}


