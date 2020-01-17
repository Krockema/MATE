using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class MinDeliveryTime : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.MinDeliveryTime.Type.Name;
        public string ArgShort => "mindt";
        public bool HasProperty => true;
        public string Description => " -MinDeliveryTime <int>";
        public Action<Configuration, string> Action { get; }

        public MinDeliveryTime()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.MinDeliveryTime(value: int.Parse(s: argument)));
            };
        }
    }
}
