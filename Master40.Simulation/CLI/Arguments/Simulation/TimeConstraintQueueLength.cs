using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class TimeConstraintQueueLength : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.TimeConstraintQueueLength.Type.Name;
        public string ArgShort => "TCQL";
        public bool HasProperty => true;
        public string Description => " -TimeConstraintQueueLength <int> : specific the length or the resourceQueues for planing reasons";
        public Action<Configuration, string> Action { get; }

        public TimeConstraintQueueLength()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.TimeConstraintQueueLength(value: int.Parse(s: argument)));
            };
        }
    }
}