using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SimulationEnd : Option<long>
    {
        public SimulationEnd(long value)
        {
            _value = value;
        }

        public SimulationEnd()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationEnd(value: long.Parse(s: argument)));
            };
        }
    }
}
