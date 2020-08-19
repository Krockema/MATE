using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Util;
using MathNet.Numerics.Distributions;

namespace Master40.DataGenerator.Generators
{
    public class ProductStructureGenerator
    {
        // Wie könnte man Testen, ob der Algorithmus dem aus SYMTEP enspricht (keine Fehler enthält)
        public ProductStructure GenerateProductStructure(InputParameterSet inputParameters)
        {
            var productStructure = new ProductStructure();
            var availableNodes = new List<HashSet<long>>();
            var nodesCounter = GeneratePartsForEachLevel(inputParameters, productStructure, availableNodes);

            var rng = new XRandom();

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
                pk.Add(new KeyValuePair<int, double>(k, 2 * (k - i) / Convert.ToDouble((depthOfAssembly - i) * (depthOfAssembly - i + 1))));
            }
            pk.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
            {
                if (o1.Value > o2.Value) return -1;
                return 1;
            });
            return pk;
        }

        private long GeneratePartsForEachLevel(InputParameterSet inputParameters, ProductStructure productStructure, List<HashSet<long>> availableNodes)
        {
            long nodesCounter = 0;
            for (var i = 1; i <= inputParameters.DepthOfAssembly; i++)
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
                for (long j = 0; j < nodeCount; j++)
                {
                    availableNodesOnThisLevel.Add(j);
                    nodesCurrentLevel[j] = new Node { AssemblyLevel = i };
                    nodesCounter++;
                }
            }

            return nodesCounter;
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
                    productStructure.Edges.Add(new Edge
                    {
                        Start = productStructure.NodesPerLevel[i][startNode],
                        End = productStructure.NodesPerLevel[i - 1][j - 1]
                    });
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
                    productStructure.Edges.Add(new Edge
                    {
                        Start = productStructure.NodesPerLevel[i - 1][j],
                        End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][posOfNode]
                    });
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
                    productStructure.Edges.Add(new Edge
                    {
                        Start = productStructure.NodesPerLevel[i - 1][j - 1],
                        End = productStructure.NodesPerLevel[i - 2][endNode]
                    });
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
                    productStructure.Edges.Add(new Edge
                    {
                        Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][posOfNode],
                        End = productStructure.NodesPerLevel[i - 1][j]
                    });
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
                productStructure.Edges.Add(new Edge
                {
                    Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][startNodePos - 1],
                    End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][endNodePos]
                });
            }
        }

        private static void DeterminationOfEdgeWeights(InputParameterSet inputParameters, ProductStructure productStructure)
        {
            foreach (var edge in productStructure.Edges)
            {
                //Änderung notwendig: µ ist nicht gleich Erwartungswert und sigma ist nicht gleich Standardabweichung²!
                edge.Weight = LogNormal.Sample(inputParameters.MeanIncomingMaterialAmount,
                    inputParameters.VarianceIncomingMaterialAmount);
            }
        }
    }
}
