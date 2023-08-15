using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class EstimatedThroughPut : Option<TimeSpan>
    {
        public static Type Type = typeof(EstimatedThroughPut);
        public EstimatedThroughPut(TimeSpan value)
        {
            _value = value;
        }

        public void Set(TimeSpan time)
        {
            _value = time;
        }
        public EstimatedThroughPut()
        {
            Action = (config, argument) => {
                config.AddOption(o: new EstimatedThroughPut(value: TimeSpan.Parse(s: argument)));
            };
        }
    }
}
