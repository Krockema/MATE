using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class MaxBucketSize : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.MaxBucketSize.Type.Name;
        public string ArgShort => "MBS";
        public bool HasProperty => true;
        public string Description => " -TimePeriodForThroughputCalculation <int> : Specify the time period considered for the calculation";
        public Action<Configuration, string> Action { get; }

        public MaxBucketSize()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.MaxBucketSize(value: long.Parse(s: argument)));
            };
        }
    }
}
