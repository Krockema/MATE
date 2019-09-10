using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments
{
    public class KpiTimeSpan : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.KpiTimeSpan.Type.Name;
        public string ArgShort => "KTS";
        public bool HasProperty => true;
        public string Description => " -KpiTimeSpan <id> : Specify the simulationId to run with";
        public Action<Configuration, string> Action { get; }

        public KpiTimeSpan()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SimulationCore.Environment.Options.KpiTimeSpan(value: int.Parse(s: argument)));
            };
        }
    }
}
