using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.External
{
    /// <summary>
    /// Starts the Simulation at
    /// </summary>
    public class IterationEndAt : ICommand
    {
        public string ArgLong => typeof(IterationEndAt).Name;
        public string ArgShort => "iea";
        public bool HasProperty => true;
        public string Description => " -IterationEndAt <int> : Specify final simulation number";
        public Action<Configuration, string> Action { get; }

        public IterationEndAt()
        {
            Action = (config, argument) => { config.AddOption(o: this); };
        }
    }
}
