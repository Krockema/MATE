using System;
using Master40.DB.Nominal;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    class SimulationKind : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SimulationKind.Type.Name;
        public string ArgShort => "simKind";
        public bool HasProperty => true;
        public string Description => " -SimulationKind <SimulationType> : Specify simulation Type <Central/Decentral>";
        public Action<Configuration, string> Action { get; }

        public SimulationKind()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(value: SimulationType.Default.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.SimulationKind(value: SimulationType.Decentral));
                }
                else if (arg.Equals(value: SimulationType.Queuing.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.SimulationKind(value: SimulationType.Central));
                }
                else
                {
                    throw  new Exception(message: "Unknown argument.");
                }
            };
        }
    }
}
