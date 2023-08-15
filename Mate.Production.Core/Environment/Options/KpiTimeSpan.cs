using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class KpiTimeSpan : Option<TimeSpan>
    {
        public static Type Type => typeof(KpiTimeSpan);
        public KpiTimeSpan(TimeSpan value)
        {
            _value = value;
        }

        public KpiTimeSpan()
        {
            Action = (config, argument) => {
                config.AddOption(o: new KpiTimeSpan(value: TimeSpan.Parse(s: argument)));
            };
        }
    }
}
