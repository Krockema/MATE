using Master40.DB;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;

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
        public static double GetPriority(this FWorkItem w, long currentTime)
        {
            return w.Priority(currentTime);
        }

        public static Func<long, double> CreateFunc(Func<long, double> func)
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
