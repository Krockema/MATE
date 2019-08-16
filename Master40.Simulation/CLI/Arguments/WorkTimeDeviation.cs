using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class WorkTimeDeviation : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.WorkTimeDeviation.Type.Name;
        public string ArgShort => "WTD";
        public bool HasProperty => true;
        public string Description => " -WorkTimeDeviation <decimal> : Specify the deviation for Operations";
        public Action<Configuration, string> Action { get; }

        public WorkTimeDeviation()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.WorkTimeDeviation(value: double.Parse(s: argument)));
            };
        }
    }
}
