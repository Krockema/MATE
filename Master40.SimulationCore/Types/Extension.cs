using System;
using System.Collections.Generic;
using static FBuckets;
using static IKeys;

namespace Master40.SimulationCore.Types
{
    public static class Extension
    {
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
            list.RemoveAt(index: list.FindIndex(match: x => x.Key == val.Key));
            list.Add(item: val);
        }
    }
}
