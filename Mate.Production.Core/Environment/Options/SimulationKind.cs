using System;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SimulationKind : Option<SimulationType>
    {
        public SimulationKind(SimulationType value)
        {
            _value = value;
        }

        public SimulationKind()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(value: SimulationType.Default.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationKind(value: SimulationType.Decentral));
                }
                else if (arg.Equals(value: SimulationType.Queuing.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationKind(value: SimulationType.Central));
                }
                else
                {
                    throw  new Exception(message: "Unknown argument.");
                }
            };
        }
    }
}
