using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class PriorityRule : Option<DataCore.Nominal.PriorityRule>
    {
        public PriorityRule(DataCore.Nominal.PriorityRule value)
        {
            _value = value;
        }

        public PriorityRule()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(value: DataCore.Nominal.PriorityRule.FIFO.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new PriorityRule(value: DataCore.Nominal.PriorityRule.FIFO));
                }
                else if (arg.Equals(value:DataCore.Nominal.PriorityRule.MDD.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new PriorityRule(value: DataCore.Nominal.PriorityRule.MDD));
                }
                else if (arg.Equals(value: DataCore.Nominal.PriorityRule.LST.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new PriorityRule(value: DataCore.Nominal.PriorityRule.LST));
                }
                else if (arg.Equals(value: DataCore.Nominal.PriorityRule.SPT.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                {
                    result.AddOption(o: new PriorityRule(value: DataCore.Nominal.PriorityRule.SPT));
                } else
                {
                    throw  new Exception(message: "Unknown argument.");
                }
            };
        }
    }
}
