using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class DistributionTest
    {
        [Fact]
        public void NormalDistributionTest()
        {
            var generator = new MeasurementValuesGenerator(1000);
            var deflectionGenerator = new DeflectionGenerator(1000);
            var numberOfUses = deflectionGenerator.AddUsage(1);
            //var angle = 10;

            using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"D:\\DistributionTest.txt"))
            {
                file.WriteLine("OneDirectionalDeflection");
                for (int i = 1; i < 501; i++)
                { 
                    //file.WriteLine(generator.GetRandomMeasurementValues(100, -2.5, 2.5, 1.95996).ToString());
                    file.WriteLine(deflectionGenerator.GetOneDirectionalDeflection(numberOfUses).ToString());
                }
                //file.WriteLine("DirectionalDeflection");
                //for (int j = 0; j < 10; j++)
                //{
                //    file.WriteLine(deflectionGenerator.GetMultiDirectionalDeflection(numberOfUses, angle).ToString());
                //    angle += 22;
                //}
            }
        }
    }
}
