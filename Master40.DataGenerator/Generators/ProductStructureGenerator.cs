using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Util;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.DataModel;
using MathNet.Numerics.Distributions;

namespace Master40.DataGenerator.Generators
{
    public class ProductStructureGenerator
    {
        // Wie könnte man Testen, ob der Algorithmus dem aus SYMTEP enspricht (keine Fehler enthält)
        public ProductStructure GenerateProductStructure(InputParameterSet inputParameters,
            MasterTableArticleType articleTypes, MasterTableUnit units, M_Unit[] unitCol)
        {
            var rng = new XRandom();
            var productStructure = new ProductStructure();
            var availableNodes = new List<HashSet<long>>();
            var nodesCounter = GenerateParts(inputParameters, productStructure, availableNodes,
                articleTypes, units, unitCol, rng);

            GenerateEdges(inputParameters, productStructure, rng, availableNodes, nodesCounter);

            DeterminationOfEdgeWeights(inputParameters, productStructure);

            return productStructure;
        }

        private List<KeyValuePair<int, double>> GetCumulatedProbabilitiesPk1(int i)
        {
            var pk = new List<KeyValuePair<int, double>>();
            for (var k = 1; k < i; k++)
            {
                pk.Add(new KeyValuePair<int, double>(k, 2 * k / Convert.ToDouble(i * (i - 1))));
            }
            pk.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
            {
                if (o1.Value > o2.Value) return -1;
                return 1;
            });
            return pk;
        }

        private Dictionary<int, List<KeyValuePair<int, double>>> GetSetOfCumulatedProbabilitiesPk1(int depthOfAssembly)
        {
            var pkPerI = new Dictionary<int, List<KeyValuePair<int, double>>>();
            for (var i = 2; i <= depthOfAssembly; i++)
            {
                pkPerI[i] = GetCumulatedProbabilitiesPk1(i);
            }
            return pkPerI;
        }

        private List<KeyValuePair<int, double>> GetCumulatedProbabilitiesPk2(int i, int depthOfAssembly)
        {
            var pk = new List<KeyValuePair<int, double>>();
            for (var k = i + 1; k <= depthOfAssembly; k++)
            {
                pk.Add(new KeyValuePair<int, double>(k,
                    2 * (k - i) / Convert.ToDouble((depthOfAssembly - i) * (depthOfAssembly - i + 1))));
            }
            pk.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
            {
                if (o1.Value > o2.Value) return -1;
                return 1;
            });
            return pk;
        }

        private long GenerateParts(InputParameterSet inputParameters, ProductStructure productStructure,
            List<HashSet<long>> availableNodes, MasterTableArticleType articleTypes, MasterTableUnit units,
            M_Unit[] unitCol, XRandom rng)
        {
            long nodesCounter = 0;
            bool sampleWorkPlanLength = inputParameters.MeanWorkPlanLength != null &&
                                        inputParameters.VarianceWorkPlanLength != null;
            Normal normalDistribution = null;
            if (sampleWorkPlanLength)
            {
                normalDistribution = Normal.WithMeanVariance((double) inputParameters.MeanWorkPlanLength,
                    (double) inputParameters.VarianceWorkPlanLength);
            }
            for (var i = 1; i <= inputParameters.DepthOfAssembly; i++)
            {
                nodesCounter += GeneratePartsForEachLevel(inputParameters, productStructure, availableNodes, articleTypes, units,
                    unitCol, rng, i, sampleWorkPlanLength, normalDistribution);
            }

            return nodesCounter;
        }

        private static long GeneratePartsForEachLevel(InputParameterSet inputParameters, ProductStructure productStructure, List<HashSet<long>> availableNodes,
            MasterTableArticleType articleTypes, MasterTableUnit units, M_Unit[] unitCol, XRandom rng, int i,
            bool sampleWorkPlanLength, Normal normalDistribution)
        {
            //Problem mit Algorithmus aus SYMTEP: bei ungünstigen Eingabeparametern gibt es auf manchen Fertigungsstufen keine Teile (0 Knoten)
            //-> Es fehlt wohl Nebenbedingung, dass Anzahl an Teilen auf jeden Fertigungsstufe mindestens 1 sein darf
            //-> Entsprechend wurde das hier angepasst
            var nodeCount = Math.Max(1, Convert.ToInt64(Math.Round(
                Math.Pow(inputParameters.ComplexityRatio / inputParameters.ReutilisationRatio, i - 1) *
                inputParameters.EndProductCount)));
            var nodesCurrentLevel = new Dictionary<long, Node>();
            productStructure.NodesPerLevel.Add(nodesCurrentLevel);
            var availableNodesOnThisLevel = new HashSet<long>();
            availableNodes.Add(availableNodesOnThisLevel);

            bool toPurchase, toBuild;
            M_Unit unit = null;
            M_ArticleType articleType;
            if (i == 1)
            {
                toPurchase = false;
                toBuild = true;
                unit = units.PIECES;
                articleType = articleTypes.PRODUCT;
            }
            else if (i == inputParameters.DepthOfAssembly)
            {
                toPurchase = true;
                toBuild = false;
                articleType = articleTypes.MATERIAL;
            }
            else
            {
                toPurchase = false;
                toBuild = true;
                unit = units.PIECES;
                articleType = articleTypes.ASSEMBLY;
            }

            for (long j = 0; j < nodeCount; j++)
            {
                unit = GeneratePartsForCurrentLevel(inputParameters, unitCol, rng, i, sampleWorkPlanLength,
                    normalDistribution, availableNodesOnThisLevel, j, unit, articleType, toPurchase, toBuild,
                    nodesCurrentLevel);
            }

            return nodeCount;
        }

        private static M_Unit GeneratePartsForCurrentLevel(InputParameterSet inputParameters, M_Unit[] unitCol, XRandom rng, int i,
            bool sampleWorkPlanLength, Normal normalDistribution, HashSet<long> availableNodesOnThisLevel, long j, M_Unit unit,
            M_ArticleType articleType, bool toPurchase, bool toBuild, Dictionary<long, Node> nodesCurrentLevel)
        {
            availableNodesOnThisLevel.Add(j);

            if (i == inputParameters.DepthOfAssembly)
            {
                var pos = rng.Next(unitCol.Length);
                unit = unitCol[pos];
            }

            var node = new Node
            {
                AssemblyLevel = i,
                Article = new M_Article
                {
                    Name = "Material " + i + "." + j,
                    ArticleTypeId = articleType.Id,
                    CreationDate = DateTime.Now,
                    DeliveryPeriod = 5,
                    UnitId = unit.Id,
                    Price = 10,
                    ToPurchase = toPurchase,
                    ToBuild = toBuild
                }
            };
            nodesCurrentLevel[j] = node;
            if (sampleWorkPlanLength && i != inputParameters.DepthOfAssembly)
            {
                int length;
                do
                {
                    length = (int) Math.Round(normalDistribution.Sample());
                } while (length < 1);

                node.WorkPlanLength = length;
            }

            return unit;
        }

        private void GenerateEdges(InputParameterSet inputParameters, ProductStructure productStructure, XRandom rng,
            List<HashSet<long>> availableNodes, long nodesCounter)
        {
            var nodesOfLastAssemblyLevelCounter =
                productStructure.NodesPerLevel[inputParameters.DepthOfAssembly - 1].LongCount();
            var edgeCount = Convert.ToInt64(Math.Round(Math.Max(
                inputParameters.ReutilisationRatio * (nodesCounter - inputParameters.EndProductCount),
                inputParameters.ComplexityRatio * (nodesCounter - nodesOfLastAssemblyLevelCounter))));
            var pkPerI = GetSetOfCumulatedProbabilitiesPk1(inputParameters.DepthOfAssembly);
            if (inputParameters.ReutilisationRatio < inputParameters.ComplexityRatio)
            {
                GenerateFirstSetOfEdgesForConvergingMaterialFlow(inputParameters, rng, pkPerI, availableNodes, productStructure);
            }
            else
            {
                GenerateFirstSetOfEdgesForDivergingMaterialFlow(inputParameters, productStructure, rng, availableNodes);
            }

            //scheinbar können hierbei Multikanten entstehen. ist das in Erzeugnisstruktur erlaubt? -> stellt kein Problem dar
            GenerateSecondSetOfEdges(inputParameters, productStructure, rng, nodesCounter, edgeCount, pkPerI);
        }

        private void GenerateFirstSetOfEdgesForConvergingMaterialFlow(InputParameterSet inputParameters, XRandom rng,
            Dictionary<int, List<KeyValuePair<int, double>>> pkPerI, List<HashSet<long>> availableNodes,
            ProductStructure productStructure)
        {
            for (var i = 1; i <= inputParameters.DepthOfAssembly - 1; i++)
            {
                for (long j = 1; j <= productStructure.NodesPerLevel[i - 1].LongCount() /*&& availableNodes[i].LongCount() > 0*/; j++)
                {
                    var startNodePos = rng.NextLong(availableNodes[i].LongCount());
                    var startNode = availableNodes[i].ToArray()[startNodePos];
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[i][startNode],
                        End = productStructure.NodesPerLevel[i - 1][j - 1]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                    availableNodes[i].Remove(startNode);
                }
            }

            for (var i = inputParameters.DepthOfAssembly; i >= 2; i--)
            {
                foreach (var j in availableNodes[i - 1])
                {
                    var u = rng.NextDouble();
                    var sum = 0.0;
                    var k = 0;
                    while (k < pkPerI[i].Count - 1)
                    {
                        sum += pkPerI[i][k].Value;
                        if (u < sum)
                        {
                            break;
                        }

                        k++;
                    }

                    var assemblyLevelOfEndNode = pkPerI[i][k].Key;
                    var posOfNode = rng.NextLong(productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1].LongCount());
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[i - 1][j],
                        End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][posOfNode]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                }
            }
        }

        private void GenerateFirstSetOfEdgesForDivergingMaterialFlow(InputParameterSet inputParameters,
            ProductStructure productStructure, XRandom rng, List<HashSet<long>> availableNodes)
        {
            for (var i = inputParameters.DepthOfAssembly; i >= 2; i--)
            {
                for (long j = 1; j <= productStructure.NodesPerLevel[i - 1].LongCount() /*&& availableNodes[i - 2].LongCount() > 0*/; j++)
                {
                    var endNodePos = rng.NextLong(availableNodes[i - 2].LongCount());
                    var endNode = availableNodes[i - 2].ToArray()[endNodePos];
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[i - 1][j - 1],
                        End = productStructure.NodesPerLevel[i - 2][endNode]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                    availableNodes[i - 2].Remove(endNode);
                }
            }

            for (var i = 1; i < inputParameters.DepthOfAssembly; i++)
            {
                var pk = GetCumulatedProbabilitiesPk2(i, inputParameters.DepthOfAssembly);
                foreach (var j in availableNodes[i - 1])
                {
                    var u = rng.NextDouble();
                    var sum = 0.0;
                    var k = 0;
                    while (k < pk.Count - 1)
                    {
                        sum += pk[k].Value;
                        if (u < sum)
                        {
                            break;
                        }

                        k++;
                    }

                    var assemblyLevelOfStartNode = pk[k].Key;
                    var posOfNode = rng.NextLong(productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].LongCount());
                    var edge = new Edge
                    {
                        Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][posOfNode],
                        End = productStructure.NodesPerLevel[i - 1][j]
                    };
                    edge.End.IncomingEdges.Add(edge);
                    productStructure.Edges.Add(edge);
                }
            }
        }

        private static void GenerateSecondSetOfEdges(InputParameterSet inputParameters, ProductStructure productStructure,
            XRandom rng, long nodesCounter, long edgeCount, Dictionary<int, List<KeyValuePair<int, double>>> pkPerI)
        {
            var possibleStartNodes = nodesCounter - inputParameters.EndProductCount;
            for (var j = productStructure.Edges.LongCount() + 1; j <= edgeCount; j++)
            {
                var startNodePos = rng.NextLong(possibleStartNodes) + 1;
                var assemblyLevelOfStartNode = 2;
                while (assemblyLevelOfStartNode < inputParameters.DepthOfAssembly)
                {
                    if (startNodePos <= productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].LongCount())
                    {
                        break;
                    }

                    startNodePos -= productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].LongCount();
                    assemblyLevelOfStartNode++;
                }

                var u = rng.NextDouble();
                var sum = 0.0;
                var k = 0;
                while (k < pkPerI[assemblyLevelOfStartNode].Count - 1)
                {
                    sum += pkPerI[assemblyLevelOfStartNode][k].Value;
                    if (u < sum)
                    {
                        break;
                    }

                    k++;
                }

                var assemblyLevelOfEndNode = pkPerI[assemblyLevelOfStartNode][k].Key;
                var endNodePos = rng.NextLong(productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1].LongCount());
                var edge = new Edge
                {
                    Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][startNodePos - 1],
                    End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][endNodePos]
                };
                edge.End.IncomingEdges.Add(edge);
                productStructure.Edges.Add(edge);
            }
        }

        private static void DeterminationOfEdgeWeights(InputParameterSet inputParameters, ProductStructure productStructure)
        {
            var logNormalDistribution = LogNormal.WithMeanVariance(inputParameters.MeanIncomingMaterialAmount,
                Math.Pow(inputParameters.StdDevIncomingMaterialAmount, 2));
            foreach (var edge in productStructure.Edges)
            {
                edge.Weight = logNormalDistribution.Sample();
            }
        }
    }
}
