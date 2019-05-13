using System;
using System.Collections.Generic;
using System.Text;
using Akka.Pattern;

namespace Master40.Simulation.CLI.Arguments
{
    public class Config : ICommand
    {
        public string ArgLong => "Config";
        public string ArgShort => "c";
        public bool HasProperty => true;
        public string Description => " -config <id> : Specify the simulationId to run with";
        public Action<ParseResult, string> Action { get; }

        public Config()
        {
            Action = (result, id) => result.ConfigId = int.Parse(id);
        }
    }
}
