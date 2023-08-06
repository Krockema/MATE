using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using System;
using System.Collections.Generic;
namespace Mate.Production.Core.Types
{
    public static class Extension
    {
        public static Func<long, double> CreateFunc(Func<long, double> func)
        {
            return func;
        }

        public static Func<BucketRecord, long, double> CreateFunc(Func<BucketRecord, long, double> func)
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
