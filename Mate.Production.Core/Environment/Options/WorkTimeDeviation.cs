using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class WorkTimeDeviation : Option<double>
    {
        public WorkTimeDeviation(double value)
        {
            _value = value;
        }

        public WorkTimeDeviation()
        {
            Action = (config, argument) => {
                config.AddOption(o: new WorkTimeDeviation(value: double.Parse(argument)));
            };
        }
    }
}
