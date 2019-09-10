using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class DBConnectionString : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.DBConnectionString.Type.Name;
        public string ArgShort => "dbCon";
        public bool HasProperty => true;
        public string Description => " -DBConnectionString <string> : Specify dbConnectionString to write results back.";
        public Action<Configuration, string> Action { get; }

        public DBConnectionString()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.DBConnectionString(value: argument));
            };
        }
    }
}
