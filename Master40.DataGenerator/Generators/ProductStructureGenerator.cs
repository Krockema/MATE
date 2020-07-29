using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DataGenerator.Model.ProductStructure;
using MathNet.Numerics.Distributions;

namespace Master40.DataGenerator.Generator
{
    public class ProductStructureGenerator
    {

        public ProductStructure GenerateProductStructure(InputParameterSet inputParameters)
        {
            ProductStructure productStructure = new ProductStructure();
            var nodesCounter = 0;
            var nodesOfLastAssemblyLevelCounter = 0;
            var availableNodes = new List<HashSet<int>>();
            for (var i = 1; i <= inputParameters.DepthOfAssembly; i++)
            {
                var nodeCount = Convert.ToInt32(Math.Round(
                    Math.Pow(inputParameters.ComplexityRatio / inputParameters.ReutilisationRatio, i - 1) *
                    inputParameters.EndProductCount));
                var nodesCurrentLevel = new List<Node>();
                productStructure.NodesPerLevel.Add(nodesCurrentLevel);
                availableNodes.Add(new HashSet<int>(Enumerable.Range(0, nodeCount)));
                for (var j = 0; j < nodeCount; j++)
                {
                    nodesCurrentLevel.Add(new Node {AssemblyLevel = i});
                    nodesCounter++;
                    if (i == inputParameters.DepthOfAssembly)
                    {
                        nodesOfLastAssemblyLevelCounter++;
                    }
                }
            }

            var rng = new Random();
            var edgeCount = Convert.ToInt32(Math.Round(Math.Max(
                inputParameters.ReutilisationRatio * (nodesCounter - inputParameters.EndProductCount),
                inputParameters.ComplexityRatio * (nodesCounter - nodesOfLastAssemblyLevelCounter))));
            var pkPerI = new Dictionary<int, List<KeyValuePair<int, double>>>();

            if (inputParameters.ReutilisationRatio < inputParameters.ComplexityRatio)
            {
                for (var i = 1; i <= inputParameters.DepthOfAssembly - 1; i++)
                {
                    for (var j = 1; j <= productStructure.NodesPerLevel[i - 1].Count /*&& availableNodes[i].Count > 0*/; i++)
                    {
                        var startNode = rng.Next(availableNodes[i].Count);
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
                    var pk = new List<KeyValuePair<int, double>>();
                    pkPerI[i] = pk;
                    for (var k = 1; k < i; k++)
                    {
                        pk.Add(new KeyValuePair<int, double>(k, 2*k / Convert.ToDouble(i * (i-1) )));
                    }
                    pk.Sort(delegate(KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
                    {
                        if (o1.Value > o2.Value) return -1;
                        return 1;
                    });
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

                        var assemblyLevelOfEndNode = pk[k].Key;
                        var posOfNode = rng.Next(productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1].Count);
                        productStructure.Edges.Add(new Edge
                        {
                            Start = productStructure.NodesPerLevel[i - 1][j],
                            End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][posOfNode]
                        });
                    }
                }
            }
            else
            {
                for (var i = inputParameters.DepthOfAssembly; i >= 2; i--)
                {
                    for (var j = 1; j <= productStructure.NodesPerLevel[i - 1].Count /*&& availableNodes[i - 2].Count > 0*/; j++)
                    {
                        var endNode = rng.Next(availableNodes[i - 2].Count);
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
                    var pk = new List<KeyValuePair<int, double>>();
                    for (var k = i + 1; k <= inputParameters.DepthOfAssembly; k++)
                    {
                        pk.Add(new KeyValuePair<int, double>(k, 2 * (k - i) / Convert.ToDouble((inputParameters.DepthOfAssembly - i) * (inputParameters.DepthOfAssembly - i + 1))));
                    }
                    pk.Sort(delegate (KeyValuePair<int, double> o1, KeyValuePair<int, double> o2)
                    {
                        if (o1.Value > o2.Value) return -1;
                        return 1;
                    });
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
                        var posOfNode = rng.Next(productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].Count);
                        productStructure.Edges.Add(new Edge
                        {
                            Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][posOfNode],
                            End = productStructure.NodesPerLevel[i - 1][j]
                        });
                    }
                }
            }

            for (var j = productStructure.Edges.Count + 1; j <= edgeCount; j++)
            {
                var possibleStartNodes = nodesCounter - inputParameters.EndProductCount;
                var startNodePos = rng.Next(possibleStartNodes) + 1;
                var assemblyLevelOfStartNode = 2;
                while (assemblyLevelOfStartNode < inputParameters.DepthOfAssembly)
                {
                    if (startNodePos <= productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].Count)
                    {
                        break;
                    }

                    assemblyLevelOfStartNode++;
                    startNodePos -= productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1].Count;
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
                var endNotePos = rng.Next(productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1].Count);
                productStructure.Edges.Add(new Edge{Start = productStructure.NodesPerLevel[assemblyLevelOfStartNode - 1][startNodePos], End = productStructure.NodesPerLevel[assemblyLevelOfEndNode - 1][endNotePos] });
            }

            foreach (var edge in productStructure.Edges)
            {
                //Änderung notwendig: µ ist nicht gleich Erwartungswert und sigma ist nicht gleich Standardabweichung²!
                edge.weight = LogNormal.Sample(inputParameters.MeanIncomingMaterialAmount,
                    inputParameters.VarianceIncomingMaterialAmount);
            }

            return productStructure;
        }
    }
}
