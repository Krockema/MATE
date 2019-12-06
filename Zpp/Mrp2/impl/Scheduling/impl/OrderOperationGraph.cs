using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.Util;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;
using Zpp.Util.StackSet;

namespace Zpp.Mrp2.impl.Scheduling.impl
{
    /**
     * The difference to DemandToProviderGraph is that no productionOrderBoms nodes are in it,
     * instead it has all operations of parent productionOrder.
     * Because scheduling(backward/forward/backward) must be done once for orders AND for operations.
     */
    public class OrderOperationGraph : DirectedGraph, IOrderOperationGraph
    {
        public OrderOperationGraph() : base()
        {
            // Don't try to remove subgraphs that rootType != customerOrderPart,
            // it's nearly impossible to correctly identify those (with performance in mind)

            // CreateGraph(dbTransactionData, aggregator);
            DemandToProviderGraph demandToProviderGraph = CreateGraph3();
            _nodes = demandToProviderGraph.GetNodes();

            if (IsEmpty())
            {
                return;
            }
        }

        /**
         * No need to traverse --> graph is ready, just do some modifications:
         * remove ProductionOrderBoms, replace ProductionOrder by operationGraph, ...
         */
        private DemandToProviderGraph CreateGraph3()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            DemandToProviderGraph demandToProviderGraph = new DemandToProviderGraph();

            
            // replace ProductionOrder by operationGraph
            foreach (var productionOrder in dbTransactionData.ProductionOrderGetAll())
            {
                if (productionOrder.IsReadOnly() == false)
                {
                    var productionOrderBomNode = new Node(productionOrder);
                    if (demandToProviderGraph.Contains(productionOrderBomNode))
                    {
                        OperationGraph operationGraph =
                            new OperationGraph((ProductionOrder) productionOrder);
                        INodes leafOfOperationGraph = operationGraph.GetLeafNodes();
                        demandToProviderGraph.ReplaceNodeByDirectedGraph(productionOrderBomNode, operationGraph);
                        /*// remove all arrows from leaf, since material must be ready to the
                        // corresponding operation not to first operation 
                        INodes successorsOfLeaf =
                            demandToProviderGraph.GetSuccessorNodes(leafOfOperationGraph.GetAny());
                        foreach (var successor in successorsOfLeaf)
                        {
                            demandToProviderGraph.RemoveEdge(leafOfOperationGraph.GetAny(), successor);
                        }*///  --> somehow not neccessary
                    }
                }
            }
            
            // connect every ProductionOrderBom successor to its operation
            foreach (var productionOrderBom in dbTransactionData.ProductionOrderBomGetAll())
            {
                if (productionOrderBom.IsReadOnly() == false)
                {
                    var productionOrderBomNode = new Node(productionOrderBom);
                    if (demandToProviderGraph.Contains(productionOrderBomNode))
                    {
                        ProductionOrderOperation productionOrderOperation =
                            ((ProductionOrderBom) productionOrderBom).GetProductionOrderOperation();
                        INodes successorNodes =
                            demandToProviderGraph.GetSuccessorNodes(productionOrderBom.GetId());
                        demandToProviderGraph.RemoveNode(productionOrderBomNode.GetId(), false);
                        foreach (var successor in successorNodes)
                        {
                            demandToProviderGraph.AddEdge(
                                new Edge(new Node(productionOrderOperation), successor));
                        }
                    }
                }
            }
            return demandToProviderGraph;
        }

        /**
         * traverse top-down and remove ProductionOrderBom, replace ProductionOrder by operationGraph
         */
        private void CreateGraph2()
        {
            IStackSet<INode> visitedProductionOrders = new StackSet<INode>();
            foreach (var rootNode in GetRootNodes())
            {
                TraverseDemandToProviderGraph(rootNode, visitedProductionOrders);
            }
        }

        private void TraverseDemandToProviderGraph(INode node,
            IStackSet<INode> visitedProductionOrders)
        {
            if (node.GetEntity().GetType() == typeof(ProductionOrderBom))
            {
                // remove, ProductionOrderBoms will be ignored and replaced by operations
                RemoveNode(node, true);
            }
            else if (node.GetEntity().GetType() == typeof(ProductionOrder) &&
                     visitedProductionOrders.Contains(node) == false)
            {
                // insert it like it is in ProductionOrderToOperationGraph

                OperationGraph operationGraph =
                    new OperationGraph((ProductionOrder) node.GetEntity());
                ReplaceNodeByDirectedGraph(node, operationGraph);
                visitedProductionOrders.Push(node);
            }

            INodes successorNodes = GetSuccessorNodes(node);
            if (successorNodes == null)
            {
                return;
            }

            foreach (var successor in successorNodes)
            {
                TraverseDemandToProviderGraph(successor, visitedProductionOrders);
            }
        }
    }
}