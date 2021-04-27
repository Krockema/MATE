using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SimulationNumber : Option<int>
    {
        public SimulationNumber(int value)
        {
            _value = value;
        }

        public SimulationNumber()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationNumber(value: int.Parse(s: argument)));
            };
        }
    }
}
