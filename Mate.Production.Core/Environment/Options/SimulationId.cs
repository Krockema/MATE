using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SimulationId : Option<int>
    {
        public SimulationId(int value)
        {
            _value = value;
        }

        public SimulationId()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationId(value: int.Parse(argument)));
            };
        }
    }
}
