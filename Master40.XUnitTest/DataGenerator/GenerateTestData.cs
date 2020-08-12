using System;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.DataModel.ProductStructure;
using Xunit;
using Xunit.Abstractions;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateTestData
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateTestData(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        //Es gibt wohl eine Diskripanz zwischen Master40 und SYMTEP was Operationen und Stücklisten (BOM) angeht (Struktur und Zeitpunkt)
        [Fact]
        public void GenerateData()
        {
            var rng = new Random();
            for (var i = 0; i < 100; i++)
            {
                //Nebenbedingung lautet, dass Fertigungstiefe mindestens 1 sein muss, es macht aber wenig Sinn, wenn sie gleich 1 ist, da es dann keine Fertigungen gibt
                //-> Anpassung der Nebenbedingung: Fertigungstiefe muss mindestens 2 sein
                var input = new InputParameterSet
                {
                    EndProductCount = rng.Next(9) + 2, DepthOfAssembly = rng.Next(10) + 1,
                    ComplexityRatio = rng.NextDouble() + 1, ReutilisationRatio = rng.NextDouble() + 1,
                    MeanIncomingMaterialAmount = 2.3, VarianceIncomingMaterialAmount = 0.7
                };
                _testOutputHelper.WriteLine(input.ToString());
                var productStructureGenerator = new ProductStructureGenerator();
                var productStructure = productStructureGenerator.GenerateProductStructure(input);

                Assert.True(productStructure.NodesPerLevel.Count == input.DepthOfAssembly &&
                            productStructure.NodesPerLevel[0].Count == input.EndProductCount);
            }
        }
    }
}
