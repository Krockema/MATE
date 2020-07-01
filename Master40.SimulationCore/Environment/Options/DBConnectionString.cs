using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class DBConnectionString : Option<string>
    {
        public static Type Type => typeof(DBConnectionString);
        public DBConnectionString(string value)
        {
            _value = value;
        }
    }
}
