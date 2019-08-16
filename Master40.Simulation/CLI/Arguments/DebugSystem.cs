using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class DebugSystem : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.DebugSystem.Type.Name;
        public string ArgShort => "DebugSystem";
        public bool HasProperty => true;
        public string Description => " -DebugSystem <id> : run the Simulation System in Debugmode";
        public Action<Configuration, string> Action { get; }
        public DebugSystem()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.DebugSystem(value: bool.Parse(value: argument)));
            };
        }
    }
}
