using Master40.SimulationCore.Environment;
using System;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class TestDataId : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.TestDataId.Type.Name;
        public string ArgShort => "tdId";
        public bool HasProperty => true;
        public string Description => " -TestDataId <int> : Specify the test data to use for simulation";
        public Action<Configuration, string> Action { get; }

        public TestDataId()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.TestDataId(value: int.Parse(s: argument)));
            };
        }
    }
}