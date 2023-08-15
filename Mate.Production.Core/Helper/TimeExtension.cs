using Akka.Hive.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mate.Production.Core.Helper
{
    public static class TimeExtension
    {

        // public static long ToSimulationTime(this Time date) => (long)(date.Value - new DateTime(2020, 1, 1)).TotalMilliseconds / 60000;
        
        // public static Time ToTime(this long x) => new Time(new DateTime(2020, 1, 1).AddMinutes(x));
        // public static TimeSpan ToTimeSpan(this long x) => TimeSpan.FromMinutes(x);

        public static DateTime ZERO = new(0);

        public static TimeSpan Sum<TSource>(this IEnumerable<TSource> enumerable,
                                            Func<TSource, TimeSpan?> func)
        {
            return enumerable.Aggregate(TimeSpan.Zero, (total, it) => total += (func(it) ?? TimeSpan.Zero));
        }

        public static DateTime? ToNullableDateTime(this long x)
        {
            return new DateTime(2020, 1, 1).AddMinutes(x);
        }
    }
}
