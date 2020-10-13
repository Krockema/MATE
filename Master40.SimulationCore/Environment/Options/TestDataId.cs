using System;
using Master40.SimulationCore.Environment.Abstractions;

namespace Master40.SimulationCore.Environment.Options
{
    public class TestDataId : Option<int>
    {
        public static Type Type => typeof(TestDataId);
        public TestDataId(int value)
        {
            _value = value;
        }
    }
}