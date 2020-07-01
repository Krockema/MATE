using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class SimulationQuantity : ICommand
    {
        public string ArgLong => typeof(SimulationQuantity).Name;
        public string ArgShort => "SQ";
        public bool HasProperty => false;
        public string Description => " -SimulationQuantity : trigger to create Average Result Set; used with Experiments to Simulate";
        public Action<Configuration, string> Action { get; }
        public SimulationQuantity()
        {
            Action = (config, argument) => { config.AddOption(this); };
        }
    }
}
