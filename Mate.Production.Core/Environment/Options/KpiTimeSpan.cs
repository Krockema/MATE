using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class KpiTimeSpan : Option<long>
    {
        public static Type Type => typeof(KpiTimeSpan);
        public KpiTimeSpan(long value)
        {
            _value = value;
        }

        public KpiTimeSpan()
        {
            Action = (config, argument) => {
                config.AddOption(o: new KpiTimeSpan(value: long.Parse(s: argument)));
            };
        }
    }
}
