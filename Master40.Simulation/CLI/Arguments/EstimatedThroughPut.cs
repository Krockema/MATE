using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class EstimatedThroughPut : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.EstimatedThroughPut.Type.Name;
        public string ArgShort => "ETP";
        public bool HasProperty => true;
        public string Description => " -EstimatedThroughPut <int> : Specify the estimated Throughput for the Productline";
        public Action<Configuration, string> Action { get; }

        public EstimatedThroughPut()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.EstimatedThroughPut(value: int.Parse(s: argument)));
            };
        }
    }
}
