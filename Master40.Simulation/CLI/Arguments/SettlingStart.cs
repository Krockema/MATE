using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class SettlingStart : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SettlingStart.Type.Name;
        public string ArgShort => "Settling";
        public bool HasProperty => true;
        public string Description => " -SettlingStart <int> : Specify the expected time to stabilize the Simulation";
        public Action<Configuration, string> Action { get; }

        public SettlingStart()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.SettlingStart(value: int.Parse(s: argument)));
            };
        }
    }
}
