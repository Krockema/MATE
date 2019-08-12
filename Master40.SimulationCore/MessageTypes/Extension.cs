using System;
using System.Collections.Generic;
using static FBuckets;
using static IJobs;
using static IKeys;

namespace Master40.SimulationCore.MessageTypes
{
    public static class Extension
    {
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="w"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static double GetPriority(this IJob w, long currentTime)
        {
            return w.Priority(currentTime);
        }

        public static Func<long, double> CreateFunc(Func<long, double> func)
        {
            return func;
        }

        public static Func<FBucket, long, double> CreateFunc(Func<FBucket, long, double> func)
        {
            return func;
        }

        public static void Replace<T>(this List<T> list, T val) where T : IKey
        { 
            list.RemoveAt(list.FindIndex(x => x.Key == val.Key));
            list.Add(val);
        }
    }
}
