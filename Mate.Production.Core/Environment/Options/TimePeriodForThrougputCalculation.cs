using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class TimePeriodForThroughputCalculation : Option<TimeSpan>
    {
        /// <summary>
        /// Specifies the time period over wich the Througput KPI is calculatet during the simulation.
        /// This has direct influence on the feedback for start of new CustomerOrders.
        /// </summary>
        public TimePeriodForThroughputCalculation(TimeSpan value)
        {
            _value = value;
        }

        public TimePeriodForThroughputCalculation()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TimePeriodForThroughputCalculation(value: TimeSpan.Parse(s: argument)));
            };
        }
    }
}
