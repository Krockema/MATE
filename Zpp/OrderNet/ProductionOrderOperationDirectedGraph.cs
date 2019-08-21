using System.Collections.Generic;
using System.Linq;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp
{
    public class ProductionOrderOperationDirectedGraph : ProductionOrderDirectedGraph,
        IDirectedGraph<INode>
    {
        /*private readonly Dictionary<ProductionOrder, IDirectedGraph<INode>>
            _directedProductionOrderOperationGraphs = new Dictionary<ProductionOrder, IDirectedGraph<INode>>();*/

        public ProductionOrderOperationDirectedGraph(IDbTransactionData dbTransactionData) : base(
            dbTransactionData)
        {
            Dictionary<ProductionOrder, IDirectedGraph<INode>>
                directedProductionOrderOperationGraphs = new Dictionary<ProductionOrder, IDirectedGraph<INode>>();
            
            foreach (var uniqueNode in GetAllUniqueNode())
            {
                ProductionOrder productionOrder = (ProductionOrder) uniqueNode.GetEntity();
                IDirectedGraph<INode> directedGraph = new DirectedGraph(_dbTransactionData);
                directedProductionOrderOperationGraphs.Add(productionOrder, directedGraph);
                Dictionary<HierarchyNumber, List<ProductionOrderOperation>>
                    hierarchyToProductionOrderOperation =
                        new Dictionary<HierarchyNumber, List<ProductionOrderOperation>>();

                List<ProductionOrderOperation> productionOrderOperations = dbTransactionData
                    .GetAggregator().GetProductionOrderOperationsOfProductionOrder(productionOrder);
                foreach (var productionOrderOperation in productionOrderOperations)
                {
                    HierarchyNumber hierarchyNumber = productionOrderOperation.GetHierarchyNumber();
                    if (hierarchyToProductionOrderOperation.ContainsKey(hierarchyNumber) == false)
                    {
                        hierarchyToProductionOrderOperation.Add(hierarchyNumber,
                            new List<ProductionOrderOperation>());
                    }

                    hierarchyToProductionOrderOperation[hierarchyNumber]
                        .Add(productionOrderOperation);
                }

                List<HierarchyNumber> hierarchyNumbers = hierarchyToProductionOrderOperation.Keys
                    .OrderByDescending(x => x.GetValue()).ToList();
                int i = 0;
                foreach (var hierarchyNumber in new HashSet<HierarchyNumber>(hierarchyNumbers))
                {
                    foreach (var productionOrderOperation in hierarchyToProductionOrderOperation[
                        hierarchyNumber])
                    {
                        if (i.Equals(0))
                        {
                            directedGraph.AddEdge(productionOrder,
                                new Edge(productionOrder, productionOrderOperation));
                        }
                        else
                        {
                            foreach (var productionOrderOperationBefore in
                                hierarchyToProductionOrderOperation[hierarchyNumbers[i - 1]])
                            {
                                directedGraph.AddEdge(productionOrderOperationBefore,
                                    new Edge(productionOrderOperationBefore, productionOrderOperation));
                            }
                        }
                    }

                    i++;
                }
            }

            _adjacencyList =
                MergeDirectedGraphs(directedProductionOrderOperationGraphs.Values.ToList())
                    .GetAdjacencyList();
        }

        /*public IDirectedGraph<INode> GetProductionOrderOperationGraphOfProductionOrder(
            ProductionOrder productionOrder)
        {
            if (_directedProductionOrderOperationGraphs.ContainsKey(productionOrder) == false)
            {
                return null;
            }

            return _directedProductionOrderOperationGraphs[productionOrder];
        }*/
    }
}