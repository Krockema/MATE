using System.Collections.Generic;
using System.Linq;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.OrderGraph
{
    public class ProductionOrderOperationDirectedGraph : ProductionOrderDirectedGraph,
        IDirectedGraph<INode>
    {
        /*private readonly Dictionary<ProductionOrder, IDirectedGraph<INode>>
            _directedProductionOrderOperationGraphs = new Dictionary<ProductionOrder, IDirectedGraph<INode>>();*/


        public ProductionOrderOperationDirectedGraph(IDbTransactionData dbTransactionData,
            ProductionOrder productionOrder) : base(dbTransactionData, false)
        {
            Dictionary<HierarchyNumber, List<ProductionOrderOperation>>
                hierarchyToProductionOrderOperation =
                    new Dictionary<HierarchyNumber, List<ProductionOrderOperation>>();

            IDirectedGraph<INode> directedGraph = new DirectedGraph(dbTransactionData);

            List<ProductionOrderOperation> productionOrderOperations = dbTransactionData
                .GetAggregator().GetProductionOrderOperationsOfProductionOrder(productionOrder);
            if (productionOrderOperations == null)
            {
                directedGraph.AddEdge(productionOrder,
                    new Edge(productionOrder, productionOrder));
                _adjacencyList = directedGraph.GetAdjacencyList();
                return;
            }
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                HierarchyNumber hierarchyNumber = productionOrderOperation.GetHierarchyNumber();
                if (hierarchyToProductionOrderOperation.ContainsKey(hierarchyNumber) == false)
                {
                    hierarchyToProductionOrderOperation.Add(hierarchyNumber,
                        new List<ProductionOrderOperation>());
                }

                hierarchyToProductionOrderOperation[hierarchyNumber].Add(productionOrderOperation);
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

            _adjacencyList = directedGraph.GetAdjacencyList();
        }

        public void RemoveProductionOrdersWithNoProductionOrderOperationsFromProductionOrderGraph(
            IDirectedGraph<INode> productionOrderGraph, ProductionOrder productionOrder)
        {
            var productionOrderOperationLeafsOfProductionOrder =
                GetLeafNodes();
            
            // if only productionOrder as node is left, delete it from productionOrderGraph
            if (productionOrderOperationLeafsOfProductionOrder == null ||
                productionOrderOperationLeafsOfProductionOrder.Any() == false)
            {
                productionOrderGraph.RemoveNode(productionOrder);
                // clear myself
                Clear();
                return;

            }
            IEnumerable<INode> leafsWithTypeProductionOrder =
                productionOrderOperationLeafsOfProductionOrder.Where(x =>
                    x.GetEntity().GetType() == typeof(ProductionOrder));
            int productionOrderCount = leafsWithTypeProductionOrder.Count();
            if (productionOrderCount > 0)
            {
                if ( productionOrderCount != 1)
                {
                    throw new MrpRunException(
                        "There can only be one root node as ProductionOrder, others must be productionOrderOperations.");
                }

                productionOrderGraph.RemoveNode(productionOrder);
                // clear myself
                Clear();
                return;
            }
        }
        
        
    }
}