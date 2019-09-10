using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class OrderArrivalRate : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.OrderArrivalRate.Type.Name;
        public string ArgShort => "simId";
        public bool HasProperty => true;
        public string Description => " -OrderArrivalRate <int> : Specify the averange timespan between two CustomerOrders";
        public Action<Configuration, string> Action { get; }
        public OrderArrivalRate()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.OrderArrivalRate(value: double.Parse(s: argument)));
            };
        }
    }
}
