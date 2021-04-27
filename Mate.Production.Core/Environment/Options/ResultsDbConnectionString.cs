using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class ResultsDbConnectionString : Option<string>
    {
        public ResultsDbConnectionString(string value)
        {
            _value = value;
        }

        public ResultsDbConnectionString()
        {
            Action = (config, argument) => {
                config.AddOption(o: new ResultsDbConnectionString(value: argument));
            };
        }
    }
}
