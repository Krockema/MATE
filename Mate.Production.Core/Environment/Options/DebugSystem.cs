using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class DebugSystem : Option<bool>
    {
        public DebugSystem(bool value)
        {
            _value = value;
        }

        public DebugSystem()
        {
            Action = (config, argument) => {
                config.AddOption(o: new DebugSystem(value: bool.Parse(value: argument)));
            };
        }
    }
}
