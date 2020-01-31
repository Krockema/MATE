using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.External
{
    public class SummarizeResults : ICommand
    {
        public string ArgLong => typeof(SummarizeResults).Name;
        public string ArgShort => "SR";
        public bool HasProperty => false;
        public string Description => " -SummarizeResults : trigger to create Average Result Set; used with ";
        public Action<Configuration, string> Action { get; }
        public SummarizeResults()
        {
            Action = (config, argument) => { config.AddOption(this); };
        }
    }
}
