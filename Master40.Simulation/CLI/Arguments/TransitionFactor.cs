using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class TransitionFactor : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.TransitionFactor.Type.Name;
        public string ArgShort => "TF";
        public bool HasProperty => true;
        public string Description => " -TransitionFactor <int> : initial Transition factor for Operations";
        public Action<Configuration, string> Action { get; }

        public TransitionFactor()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.TransitionFactor(value: int.Parse(s: argument)));
            };
        }
    }
}
