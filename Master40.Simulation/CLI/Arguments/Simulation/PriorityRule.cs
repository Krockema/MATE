using System;
using Master40.SimulationCore.Environment;

namespace Master40.Simulation.CLI.Arguments.Simulation
{
    class PriorityRule : ICommand
    {
        public string ArgLong => SimulationCore.Environment.Options.PriorityRule.Type.Name;
        public string ArgShort => "simKind";
        public bool HasProperty => true;
        public string Description => " -SimulationKind <SimulationType> : Specify simulation Type <Central/Decentral>";
        public Action<Configuration, string> Action { get; }

        public PriorityRule()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(value: DB.Nominal.PriorityRule.FIFO.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.PriorityRule(value: DB.Nominal.PriorityRule.FIFO));
                }
                else if (arg.Equals(value:DB.Nominal.PriorityRule.MDD.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.PriorityRule(value: DB.Nominal.PriorityRule.MDD));
                }
                else if (arg.Equals(value: DB.Nominal.PriorityRule.LST.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.PriorityRule(value: DB.Nominal.PriorityRule.LST));
                }
                else if (arg.Equals(value: DB.Nominal.PriorityRule.SPT.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new SimulationCore.Environment.Options.PriorityRule(value: DB.Nominal.PriorityRule.SPT));
                } else
                {
                    throw  new Exception(message: "Unknown argument.");
                }
            };
        }
    }
}
