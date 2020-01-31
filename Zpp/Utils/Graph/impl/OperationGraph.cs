using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DataLayer;
using Zpp.DataLayer.impl;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Mrp2.impl.Scheduling.impl;
using Zpp.Util.StackSet;

namespace Zpp.Util.Graph.impl
{
    /**
     * root is production order, followed by operations ordered by hierarchyNumber
     */
    public class OperationGraph : DirectedGraph
    {
        public OperationGraph(ProductionOrder productionOrder) : base()
        {
            CreateGraph2(productionOrder);
        }
        
        public OperationGraph(OrderOperationGraph orderOperationGraph) : base()
        {
            IStackSet<IGraphNode> nodes = new StackSet<IGraphNode>(orderOperationGraph.GetNodes());
            foreach (var graphNode in nodes)
            {
                if (graphNode.GetNode().GetEntity().GetType() != typeof(ProductionOrderOperation))
                {
                    orderOperationGraph.RemoveNode(graphNode.GetNode(), true);
                }
            }

            _nodes = orderOperationGraph.GetNodes();
        }

        private void CreateGraph2(ProductionOrder productionOrder)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            IEnumerable<ProductionOrderOperation> productionOrderOperations = dbTransactionData
                .ProductionOrderOperationGetAll().GetAll().Where(x =>
                    x.GetValue().ProductionOrderId.Equals(productionOrder.GetId().GetValue()))
                .OrderByDescending(x => x.GetHierarchyNumber().GetValue());
            ;
            if (productionOrderOperations.Any() == false)
            {
                Clear();
                return;
            }

            // root is always the productionOrder
            INode predecessor = new Node(productionOrder);
            foreach (var operation in productionOrderOperations)
            {
                INode operationNode = new Node(operation);
                AddEdge(new Edge(predecessor, operationNode));
                predecessor = operationNode;
            }
        }
    }
}