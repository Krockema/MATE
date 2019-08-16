using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class Seed : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.Seed.Type.Name;
        public string ArgShort => "seed";
        public bool HasProperty => true;
        public string Description => " -Seed <int> : Specify the seed for the random number generator";
        public Action<Configuration, string> Action { get; }

        public Seed()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.Seed(value: int.Parse(s: argument)));
            };
        }
    }
}
