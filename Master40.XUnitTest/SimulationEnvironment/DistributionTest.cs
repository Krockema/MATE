using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class DistributionTest
    {
        [Fact(Skip = "Local Test")]
        public void NormalDistributionTest()
        {
            var generator = new MeasurementValuesGenerator(1000);

            using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"D:\\DistributionTest.txt"))
            {
                for (int i = 0; i < 1000; i++)
                {
                    file.WriteLine(generator.GetRandomMeasurementValues(100, -2.5, 2.5, 1.95996).ToString());

                }
            }
        }
    }
}
