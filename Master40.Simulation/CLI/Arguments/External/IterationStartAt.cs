using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.External
{
    /// <summary>
    /// Starts the Simulation at
    /// </summary>
    public class IterationStartAt : ICommand
    {
        public string ArgLong => typeof(IterationStartAt).Name;
        public string ArgShort => "isa";
        public bool HasProperty => true;
        public string Description => " -IterationStartAt <int> : Specify start simulation number";
        public Action<Configuration, string> Action { get; }

        public IterationStartAt()
        {
            Action = (config, argument) => { config.AddOption(o: this); };
        }
    }
}
