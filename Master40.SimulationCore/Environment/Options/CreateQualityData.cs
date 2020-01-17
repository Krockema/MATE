using Master40.SimulationCore.Environment.Abstractions;
using System;

namespace Master40.SimulationCore.Environment.Options
{
    public class CreateQualityData : Option<bool>
    {
        public static Type Type => typeof(CreateQualityData);
        public CreateQualityData(bool value)
        {
            _value = value;
        }
    }
}
