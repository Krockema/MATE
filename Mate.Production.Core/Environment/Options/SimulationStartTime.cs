using Mate.Production.Core.Environment.Abstractions;
using System;

namespace Mate.Production.Core.Environment.Options
{
    public class SimulationStartTime : Option<DateTime>
    {
        public SimulationStartTime(DateTime value)
        {
            _value = value;
        }

        public SimulationStartTime()
        {
            Action = (config, argument) =>
            {
                config.AddOption(o: new SimulationStartTime(value: DateTime.Parse(s: argument)));
            };
        }
    }
}
