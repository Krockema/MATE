using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    public class TimeToAdvance : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.TimeToAdvance.Type.Name;
        public string ArgShort => "timeToAdvance";
        public bool HasProperty => true;
        public string Description => " -timeToAdvance <long> : min Time span between two simulation steps in milliseconds";
        public Action<Configuration, string> Action { get; }

        public TimeToAdvance()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.TimeToAdvance(value: TimeSpan.FromMilliseconds(long.Parse(argument))));
            };
        }
    }
}