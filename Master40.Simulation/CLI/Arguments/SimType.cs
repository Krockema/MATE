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
                if (arg.Equals(SimulationType.Decentral.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(new SimulationCore.Environment.Options.SimulationKind(SimulationType.Decentral));
                }
                else if (arg.Equals(SimulationType.Central.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(new SimulationCore.Environment.Options.SimulationKind(SimulationType.Central));
                }
                else if (arg.Equals(SimulationType.Bucket.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.SimulationType = SimulationType.Bucket;
                }
                else
                {
                    throw  new Exception("Unknown argument.");
                }
            };
        }
    }
}
