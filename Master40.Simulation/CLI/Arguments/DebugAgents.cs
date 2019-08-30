using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class DebugAgents : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.DebugAgents.Type.Name;
        public string ArgShort => "debugAgents";
        public bool HasProperty => true;
        public string Description => " -debugAgents <agents> : run with full Log from Agentsbehaviour";
        public Action<Configuration, string> Action { get; }

        public DebugAgents()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.DebugAgents(value: bool.Parse(value: argument)));
            };
        }
    }
}
