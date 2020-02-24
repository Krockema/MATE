using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.External
{
    public class StartHangfire : ICommand
    {
        public bool Silent { get; set; }
        public string ArgLong => typeof(StartHangfire).Name;
        public string ArgShort => "HFS";
        public bool HasProperty => true;
        public string Description => " -StartHangfire <bool> : Specify ConnectionString to access Hangfire Jobqueue.";
        public Action<Configuration, string> Action { get; }
        public StartHangfire()
        {
            
            Action = (config, argument) =>
            {
                this.Silent = argument != null && bool.Parse(argument);
                config.AddOption(this);

            };
        }
    }
}
