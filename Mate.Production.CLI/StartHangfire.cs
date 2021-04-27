using System;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.CLI
{
    public class StartHangfire : IOption<bool>, ICommand
    {
        public bool Silent { get; set; }
        public string ArgLong => nameof(StartHangfire);
        public string ArgShort => "HFS";
        public bool HasProperty => true;
        public string Description => " -StartHangfire <bool> : Specify ConnectionString to access Hangfire Jobqueue.";
        public Action<Configuration, string> Action { get; }

        public bool Value => true;

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
