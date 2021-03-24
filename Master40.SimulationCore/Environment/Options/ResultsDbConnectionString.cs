using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class ResultsDbConnectionString : Option<string>
    {
        public static Type Type => typeof(ResultsDbConnectionString);
        public ResultsDbConnectionString(string value)
        {
            _value = value;
        }
    }
}
