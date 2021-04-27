using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class EstimatedThroughPut : Option<long>
    {
        public static Type Type = typeof(EstimatedThroughPut);
        public EstimatedThroughPut(long value)
        {
            _value = value;
        }

        public void Set(long time)
        {
            _value = time;
        }
        public EstimatedThroughPut()
        {
            Action = (config, argument) => {
                config.AddOption(o: new EstimatedThroughPut(value: long.Parse(s: argument)));
            };
        }
    }
}
