using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class SimulationEnd : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SimulationEnd.Type.Name;
        public string ArgShort => "simEnd";
        public bool HasProperty => true;
        public string Description => " -SimulationEnd <long> : Specify the simulation end time";
        public Action<Configuration, string> Action { get; }

        public SimulationEnd()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.SimulationEnd(value: int.Parse(s: argument)));
            };
        }
    }
}
