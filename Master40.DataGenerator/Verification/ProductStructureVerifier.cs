using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.GeneratorModel;

namespace Master40.DataGenerator.Verification
{
    public class ProductStructureVerifier
    {

        public void VerifyComplexityAndReutilizationRation(ProductStructureInput input, ProductStructure productStructure)
        {
            var nodesOfLastAssemblyLevelCounter =
                productStructure.NodesPerLevel[input.DepthOfAssembly - 1].LongCount();
            var actualComplexityRatio = productStructure.Edges.Count /
                                        (double) (productStructure.NodesCounter - nodesOfLastAssemblyLevelCounter);
            var actualReutilizationRation = productStructure.Edges.Count /
                                            (double) (productStructure.NodesCounter - input.EndProductCount);
        }

    }
}