using System;
using Master40.DB.Enums;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    class SimulationKind : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SimulationKind.Type.Name;
        public string ArgShort => "simKind";
        public bool HasProperty => true;
        public string Description => " -SimulationKind <Type> : Specify simulation Type <Central/Decentral>";
        public Action<Configuration, string> Action { get; }

        public SimulationKind()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(value: SimulationType.Decentral.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.SimulationKind(value: SimulationType.Decentral));
                }
                else if (arg.Equals(value: SimulationType.Central.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.SimulationKind(value: SimulationType.Central));
                }
                else if (arg.Equals(value: SimulationType.Bucket.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.SimulationKind(value: SimulationType.Bucket));
                }
                else
                {
                    throw  new Exception(message: "Unknown argument.");
                }
            };
        }
    }
}
