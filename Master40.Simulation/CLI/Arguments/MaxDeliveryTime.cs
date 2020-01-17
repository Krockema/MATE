using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class MaxDeliveryTime : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.MaxDeliveryTime.Type.Name;
        public string ArgShort => "maxdt";
        public bool HasProperty => true;
        public string Description => " -MaxDeliveryTime <int>";
        public Action<Configuration, string> Action { get; }

        public MaxDeliveryTime()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.MaxDeliveryTime(value: int.Parse(s: argument)));
            };
        }
    }
}
