using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class SimulationNumber : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SimulationNumber.Type.Name;
        public string ArgShort => "simNr";
        public bool HasProperty => true;
        public string Description => " -SimulationNumber <nr> : Specify the simulationNumber to run with";
        public Action<Configuration, string> Action { get; }

        public SimulationNumber()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.SimulationNumber(value: int.Parse(s: argument)));
            };
        }
    }
}
