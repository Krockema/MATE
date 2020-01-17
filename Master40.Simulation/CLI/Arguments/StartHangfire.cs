using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class StartHangfire : ICommand
    {
        public string ArgLong => typeof(StartHangfire).Name;
        public string ArgShort => "HFS";
        public bool HasProperty => false;
        public string Description => " -HangfireConnectionString <string> : Specify ConnectionString to access Hangfire Jobqueue.";
        public Action<Configuration, string> Action { get; }
        public StartHangfire()
        {
            Action = (config, argument) => { config.AddOption(this); };
        }
    }
}
