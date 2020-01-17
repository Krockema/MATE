using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class TimePeriodForThroughputCalculation : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.TimePeriodForThroughputCalculation.Type.Name;
        public string ArgShort => "TFTC";
        public bool HasProperty => true;
        public string Description => " -TimePeriodForThroughputCalculation <int> : Specify the time period considered for calculation";
        public Action<Configuration, string> Action { get; }

        public TimePeriodForThroughputCalculation()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.TimePeriodForThroughputCalculation(value: int.Parse(s: argument)));
            };
        }
    }
}
