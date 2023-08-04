using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class GANTTPLANOptRunnerPath : Option<string>
    {
        public GANTTPLANOptRunnerPath(string value)
        {
            _value = value;
        }

        public GANTTPLANOptRunnerPath()
        {
            Action = (config, argument) => {
                config.AddOption(o: new GANTTPLANOptRunnerPath(value: argument));
            };
        }
    }
}
