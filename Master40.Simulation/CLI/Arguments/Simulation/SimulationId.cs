using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class SimulationId : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SimulationId.Type.Name;
        public string ArgShort => "simId";
        public bool HasProperty => true;
        public string Description => " -SimulationId <int> : Specify the simulationId to run with";
        public Action<Configuration, string> Action { get; }

        public SimulationId()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.SimulationId(value: int.Parse(s: argument)));
            };
        }
    }
}
