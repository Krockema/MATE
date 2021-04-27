using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class TransitionFactor : Option<decimal>
    {
        public TransitionFactor(decimal value)
        {
            _value = value;
        }

        public TransitionFactor()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TransitionFactor(value: decimal.Parse(argument)));
            };
        }
    }
}
