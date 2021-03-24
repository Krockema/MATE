using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class ResultsDbConnectionString : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.ResultsDbConnectionString.Type.Name;
        public string ArgShort => "dbConResults";
        public bool HasProperty => true;
        public string Description => " -DBConnectionString <string> : Specify dbConnectionString to write results back.";
        public Action<Configuration, string> Action { get; }

        public ResultsDbConnectionString()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.ResultsDbConnectionString(value: argument));
            };
        }
    }
}
