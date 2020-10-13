using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Master40.DB.Data.Context;
using Master40.DB.Data.DynamicInitializer.Tables;
using Master40.DB.Util;

namespace Master40.DataGenerator.Verification
{
    public class TransitionMatrixGeneratorVerifier
    {

        public double? ActualOrganizationDegree;
        public double? GeneratedOrganizationDegree;

        public void VerifyGeneratedData(TransitionMatrix transitionMatrix, List<Dictionary<long, Node>> nodesPerLevel,
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

        public void VerifySimulatedData(MasterDBContext dbContext, DataGeneratorContext dbGeneratorCtx,
            ResultContext dbResultCtx, int simNumber)
        {
            var simulation = SimulationRepository.GetSimulationById(simNumber, dbGeneratorCtx);
            if (simulation != null)
            {
                var approach = ApproachRepository.GetApproachById(dbGeneratorCtx, simulation.ApproachId);
                if (approach.TransitionMatrixInput.ExtendedTransitionMatrix)
                {
                    var articleCount =
                        ArticleRepository.GetArticleNamesAndCountForEachUsedArticleInSimulation(dbResultCtx, simNumber);

                    var articlesByNames =
                        ArticleRepository.GetArticlesByNames(articleCount.Keys.ToHashSet(), dbContext);
                    var capabilities = ResourceCapabilityRepository.GetParentResourceCapabilities(dbContext);

                    var actualTransitionMatrix = new TransitionMatrix
                    {
                        Pi = new double[capabilities.Count + 1, capabilities.Count + 1]
                    };

                    var capPosByCapId = new Dictionary<int, int>();
                    foreach (var cap in capabilities)
                    {
                        var number = cap.Name.Substring(0, cap.Name.IndexOf(" "));
                        var pos = AlphabeticNumbering.GetNumericRepresentation(number);
                        capPosByCapId.Add(cap.Id, pos);
                    }

                    foreach (var a in articlesByNames)
                    {
                        var operations = a.Value.Operations.ToList();
                        operations.Sort((o1, o2) => o1.HierarchyNumber.CompareTo(o2.HierarchyNumber));

                        var operationCount = 0;
                        var lastCapPos = 0;
                        do
                        {
                            var capPos =
                                capPosByCapId[
                                    operations[operationCount].ResourceCapability.ParentResourceCapability.Id];
                            actualTransitionMatrix.Pi[lastCapPos, capPos] += articleCount[a.Key];
                            lastCapPos = capPos + 1;
                            operationCount++;
                        } while (operationCount < operations.Count);

                        actualTransitionMatrix.Pi[lastCapPos, capabilities.Count] += articleCount[a.Key];
                    }

                    for (var i = 0; i <= capabilities.Count; i++)
                    {
                        var sum = 0.0;
                        for (var j = 0; j <= capabilities.Count; j++)
                        {
                            sum += actualTransitionMatrix.Pi[i, j];
                        }

                        for (var j = 0; j <= capabilities.Count; j++)
                        {
                            actualTransitionMatrix.Pi[i, j] /= sum;
                        }
                    }

                    var generator = new MainGenerator();
                    generator.StartGeneration(approach, dbContext, dbResultCtx);

                    var transitionMatrixGenerator = new TransitionMatrixGenerator();
                    ActualOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(
                        actualTransitionMatrix.Pi,
                        capabilities.Count + 1);
                    GeneratedOrganizationDegree = transitionMatrixGenerator.CalcOrganizationDegree(
                        generator.TransitionMatrix.Pi,
                        capabilities.Count + 1);
                }
            }
        }
    }
}