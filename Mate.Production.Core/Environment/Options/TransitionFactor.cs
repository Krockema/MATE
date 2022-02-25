using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class TransitionFactor : Option<double>
    {
        public TransitionFactor(double value)
        {
            _value = value;
        }

        public TransitionFactor()
        {
            Action = (config, argument) => {
                config.AddOption(o: new TransitionFactor(value: double.Parse(argument)));
            };
        }
    }
}
