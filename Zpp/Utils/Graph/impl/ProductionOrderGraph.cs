using System;
using System.Collections.Generic;
using System.Linq;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Util.StackSet;

namespace Zpp.Util.Graph.impl
{
    public class ProductionOrderGraph : DemandToProviderGraph
    {
        public ProductionOrderGraph(bool includeReadOnlyProductionOrders) : base()
        {
            if (IsEmpty())
            {
                return;
            }

            // CreateGraphOld();

            CreateGraph();

            // CreateGraph2(); --> non-recursive, but doesn't works correctly

            if (includeReadOnlyProductionOrders == false)
            {
                List<IGraphNode> copyOfGraphNodes = new List<IGraphNode>();
                copyOfGraphNodes.AddRange(_nodes);
                foreach (var node in copyOfGraphNodes)
                {
                    if (node.GetNode().GetEntity().IsReadOnly())
                    {
                        RemoveNode(node.GetNode(), true);
                    }
                }
            }
        }
        
        private void CreateGraph()
        {
            List<IEdge> edges = new List<IEdge>();
            foreach (var rootNode in GetRootNodes())
            {
                Traverse( rootNode,  null,  edges);
            }
            Clear();
            AddEdges(edges);
        }

        private void Traverse(INode node, INode lastProductionOrder, List<IEdge> edges)
        {
            if (node.GetEntity().GetType() == typeof(ProductionOrder))
            {
                if (lastProductionOrder != null)
                {
                    // connect
                    edges.Add(new Edge(lastProductionOrder, node));
                }
                lastProductionOrder = node;
            }
            
            
            INodes successorNodes = GetSuccessorNodes(node);
            if (successorNodes != null)
            {
                foreach (var successorNode in successorNodes)
                {
                    Traverse(successorNode, lastProductionOrder,  edges);
                }
            }
        }
        
        private void CreateGraph2()
        {
            INodes roots = GetRootNodes();
            StackSet<IEdge> newEdges = new StackSet<IEdge>();
            foreach (var rootNode in roots)
            {
                newEdges.PushAll(CreateGraphFor(rootNode, typeof(ProductionOrder), this));
            }
            Clear();
            AddEdges(newEdges);
        }
        
        /** basic principle:
         * 
         * put A on the list -> {A}
            put B on the list -> {A,B}
            put C on the list -> {A,B,C}
            since C is a leaf, print the list (A,B,C)
            remove C from the list -> {A,B}
            put D on the list -> {A,B,D}
            since D is a leaf, print the list (A,B,D)
         */
        public static List<IEdge> CreateGraphFor(INode root, Type type, IDirectedGraph<INode> directedGraph)
        {
            Stack<INode> stack = new Stack<INode>();
            Stack<INode> pathProductionOrders = new Stack<INode>();
            
            INode lastProductionOrder = null;
            List<IEdge> edges = new List<IEdge>();
            
            stack.Push(root);
            while( stack.Any() ) {
                // Do something
                INode popped = stack.Pop();
                
                if (popped.GetEntity().GetType() == type)
                {
                    pathProductionOrders.Push(popped);
                    if (lastProductionOrder == null)
                    {
                        lastProductionOrder = popped;
                    }
                    else
                    {
                        edges.Add(new Edge(lastProductionOrder, popped));
                        lastProductionOrder = popped;
                    }
                }

                // Push other objects on the stack.
                INodes successorNodes = directedGraph.GetSuccessorNodes(popped);
                if (successorNodes != null)
                {
                    foreach (var successorNode in successorNodes)
                    {
                        stack.Push(successorNode);
                    }
                }
                // popped is a leaf, remove it from path if it was a PrO
                else
                {
                    if (lastProductionOrder != null && lastProductionOrder.Equals(popped))
                    {
                        pathProductionOrders.Pop();
                        if (pathProductionOrders.Any())
                        {
                            lastProductionOrder = pathProductionOrders.Pop();
                            pathProductionOrders.Push(lastProductionOrder);
                        }
                    }
                }
                
            }

            return edges;
        }

        private void CreateGraphOld( )
        {
            foreach (var uniqueNode in GetAllUniqueNodes())
            {
                if (uniqueNode.GetEntity().GetType() != typeof(ProductionOrder)
                    // && uniqueNode.GetEntity().GetType() != typeof(ProductionOrderBom)
                )
                {
                    RemoveNode(uniqueNode, true);
                }
                /*else
                {
                    if (includeProductionOrdersWithoutOperations == false)
                    {
                        ProductionOrder productionOrder = (ProductionOrder) uniqueNode.GetEntity();
                        if (ZppConfiguration.CacheManager.GetAggregator()
                                .GetProductionOrderOperationsOfProductionOrder(productionOrder) ==
                            null)
                        {
                            RemoveNode(uniqueNode);
                        }
                    }
                }*/
            }
        }
    }
}