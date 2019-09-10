using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class TimePeriodForThrougputCalculation : Option<long>
    {
        public static Type Type => typeof(TimePeriodForThrougputCalculation);
        /// <summary>
        /// Specifies the time period over wich the Througput KPI is calculatet during the simulation.
        /// This has direct influence on the feedback for start of new CustomerOrders.
        /// </summary>
        public TimePeriodForThrougputCalculation(long value)
        {
            _value = value;
        }
    }
}
