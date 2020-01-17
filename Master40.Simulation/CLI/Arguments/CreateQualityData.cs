using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class CreateQualityData : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.CreateQualityData.Type.Name;
        public string ArgShort => "CreateQualityData";
        public bool HasProperty => true;
        public string Description => " -CreateQualityData <id> : Specify the simulation results are saved to Database";
        public Action<Configuration, string> Action { get; }

        public CreateQualityData()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.CreateQualityData(value: bool.Parse(value: argument)));
            };
        }
    }
}
