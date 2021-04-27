using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class MaxBucketSize : Option<long>
    {
        public MaxBucketSize(long value)
        {
            _value = value;
        }

        public MaxBucketSize()
        {

            Action = (config, argument) => {
                config.AddOption(o: new MaxBucketSize(value: long.Parse(s: argument)));
            };
        }
    }
}