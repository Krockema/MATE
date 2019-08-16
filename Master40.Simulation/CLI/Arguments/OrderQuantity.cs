using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class OrderQuantity : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.OrderQuantity.Type.Name;
        public string ArgShort => "oqty";
        public bool HasProperty => true;
        public string Description => " -OrderQuantity <int> : Specify the number of orders to create";
        public Action<Configuration, string> Action { get; }

        public OrderQuantity()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.OrderQuantity(value: int.Parse(s: argument)));
            };
        }
    }
}
