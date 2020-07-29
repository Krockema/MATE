using Master40.DataGenerator.Generator;
using Master40.DataGenerator.Model.ProductStructure;
using Xunit;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateTestData
    {
        [Fact]
        public void GenerateData()
        {
            var input = new InputParameterSet
            {
                EndProductCount = 5, DepthOfAssembly = 7, ComplexityRatio = 1.7, ReutilisationRatio = 1.4,
                MeanIncomingMaterialAmount = 2.3, VarianceIncomingMaterialAmount = 0.7
            };
            var productStructureGenerator = new ProductStructureGenerator();
            var productStructure = productStructureGenerator.GenerateProductStructure(input);

            Assert.True(productStructure.NodesPerLevel.Count == input.DepthOfAssembly && productStructure.NodesPerLevel[0].Count == input.EndProductCount);
        }
    }
}
