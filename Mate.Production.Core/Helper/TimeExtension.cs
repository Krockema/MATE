using Akka.Hive.Definitions;
using System;

namespace Mate.Production.Core.Helper
{
    public static class TimeExtension
    {

        public static long ToSimulationTime(this Time date) => (long)(date.Value - new DateTime(2020, 1, 1)).TotalMilliseconds / 60000;
        
        public static Time ToTime(this long x) => new Time(new DateTime(2020, 1, 1).AddMinutes(x));
        public static TimeSpan ToTimeSpan(this long x) => TimeSpan.FromMinutes(x);


        public static DateTime? ToNullableDateTime(this long x)
        {
            return new DateTime(2020, 1, 1).AddMinutes(x);
        }
    }
}
