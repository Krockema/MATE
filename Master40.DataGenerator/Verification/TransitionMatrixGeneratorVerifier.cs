using System.Collections.Generic;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.DataModel.TransitionMatrix;
using Master40.DataGenerator.Generators;
using Master40.DB.Data.DynamicInitializer.Tables;

namespace Master40.DataGenerator.Verification
{
    public class TransitionMatrixGeneratorVerifier
    {

        public double ActualOrganizationDegree;
        public double GeneratedOrganizationDegree;

        public void Verify(TransitionMatrix transitionMatrix, List<Dictionary<long, Node>> nodesPerLevel,
            MasterTableResourceCapability capabilities)
        {

            var actualTransitionMatrix = new TransitionMatrix
            {
                Pi = new double[capabilities.ParentCapabilities.Count + 1, capabilities.ParentCapabilities.Count + 1]
            };
            for (var i = 0; i < nodesPerLevel.Count - 1; i++)
            {
                foreach (var article in nodesPerLevel[i].Values)
                {
                    var operationCount = 0;
                    var lastCapPos = 0;
                    do
                    {
                        var capPos = capabilities.ParentCapabilities.FindIndex(x =>
                            object.ReferenceEquals(x,
                                article.Operations[operationCount].MOperation.ResourceCapability.ParentResourceCapability));
                        actualTransitionMatrix.Pi[lastCapPos, capPos]++;
                        lastCapPos = capPos + 1;
                        operationCount++;
                    } while (operationCount < article.Operations.Count);

                    actualTransitionMatrix.Pi[lastCapPos, capabilities.ParentCapabilities.Count]++;
                }
            }

            for (var i = 0; i <= capabilities.ParentCapabilities.Count; i++)
            {
                var sum = 0.0;
                for (var j = 0; j <= capabilities.ParentCapabilities.Count; j++)
                {
                    sum += actualTransitionMatrix.Pi[i, j];
                }

                for (var j = 0; j <= capabilities.ParentCapabilities.Count; j++)
                {
                    actualTransitionMatrix.Pi[i, j] /= sum;
                }
            }

            var transitionMatrixGenerator = new TransitionMatrixGenerator();
            ActualOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(actualTransitionMatrix.Pi,
                capabilities.ParentCapabilities.Count + 1);
            GeneratedOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(transitionMatrix.Pi,
                capabilities.ParentCapabilities.Count + 1);
        }
    }
}