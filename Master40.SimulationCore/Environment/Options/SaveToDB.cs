using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class SaveToDB : Option<bool>
    {
        public static Type Type => typeof(SaveToDB);
        public SaveToDB(bool value)
        {
            _value = value;
        }
    }
}
