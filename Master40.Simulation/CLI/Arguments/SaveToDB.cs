using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class SaveToDB : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.SaveToDB.Type.Name;
        public string ArgShort => "SaveDB";
        public bool HasProperty => true;
        public string Description => " -SaveToDB <id> : Specify the simulation results are saved to Database";
        public Action<Configuration, string> Action { get; }

        public SaveToDB()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.SaveToDB(value: bool.Parse(value: argument)));
            };
        }
    }
}
