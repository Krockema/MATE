using Master40.DB.Nominal;
using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class PriorityRule : Option<DB.Nominal.PriorityRule>
    {
        public static Type Type => typeof(PriorityRule);
        public PriorityRule(DB.Nominal.PriorityRule value)
        {
            _value = value;
        }
    }
}
