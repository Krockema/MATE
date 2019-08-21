using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.GraphicalRepresentation;
using Zpp.MachineDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public class DirectedGraph : IDirectedGraph<INode>
    {
        protected Dictionary<INode, List<IEdge>> _adjacencyList =
            new Dictionary<INode, List<IEdge>>();

        protected readonly IDbTransactionData _dbTransactionData;

        public DirectedGraph(IDbTransactionData dbTransactionData)
        {
            _dbTransactionData = dbTransactionData;
        }

        public INodes GetSuccessorNodes(INode tailNode)
        {
            if (!_adjacencyList.ContainsKey(tailNode) || _adjacencyList[tailNode].Any() == false)
            {
                return null;
            }

            return new Nodes(_adjacencyList[tailNode].Select(x => x.GetHeadNode()).ToList());
        }

        public INodes GetPredecessorNodes(INode headNode)
        {
            List<INode> predecessorNodes = new List<INode>();
            if (GetAllEdgesTowardsHeadNode(headNode) == null)
            {
                return null;
            }

            foreach (var edge in GetAllEdgesTowardsHeadNode(headNode))
            {
                predecessorNodes.Add(edge.GetTailNode());
            }

            return new Nodes(predecessorNodes);
        }

        public void AddEdges(INode fromNode, List<IEdge> edges)
        {
            if (!_adjacencyList.ContainsKey(fromNode))
            {
                _adjacencyList.Add(fromNode, edges);
                return;
            }

            _adjacencyList[fromNode].AddRange(edges);
        }

        public void AddEdge(INode fromNode, IEdge edge)
        {
            if (!_adjacencyList.ContainsKey(fromNode))
            {
                _adjacencyList.Add(fromNode, new List<IEdge>());
            }

            _adjacencyList[fromNode].Add(edge);
        }

        public int CountEdges()
        {
            return GetAllHeadNodes().Count();
        }

        public List<IEdge> GetAllEdgesFromTailNode(INode tailNode)
        {
            if (_adjacencyList.ContainsKey(tailNode) == false)
            {
                return null;
            }

            return _adjacencyList[tailNode];
        }

        public List<IEdge> GetAllEdgesTowardsHeadNode(INode headNode)
        {
            List<IEdge> edgesTowardsHeadNode = new List<IEdge>();
            foreach (var tailNode in GetAllTailNodes())
            {
                foreach (var edge in _adjacencyList[tailNode])
                {
                    if (edge.GetHeadNode().Equals(headNode))
                    {
                        edgesTowardsHeadNode.Add(edge);
                    }
                }
            }

            if (edgesTowardsHeadNode.Any() == false)
            {
                return null;
            }

            return edgesTowardsHeadNode;
        }

        public override string ToString()
        {
            string mystring = "";
            foreach (var fromNode in GetAllTailNodes())
            {
                foreach (var edge in GetAllEdgesFromTailNode(fromNode))
                {
                    // <Type>, <Menge>, <ItemName> and on edges: <Menge>
                    Quantity quantity = null;
                    if (edge.GetDemandToProvider() != null)
                    {
                        quantity = new Quantity(edge.GetDemandToProvider().Quantity);
                    }

                    mystring +=
                        $"\"{fromNode.GetId()};{fromNode.GetGraphizString(_dbTransactionData)}\" -> " +
                        $"\"{edge.GetHeadNode().GetId()};{edge.GetHeadNode().GetGraphizString(_dbTransactionData)}\"";
                    // if (quantity.IsNull() == false)
                    if (quantity != null && quantity.IsNull() == false)
                    {
                        mystring += $" [ label=\" {quantity}\" ]";
                    }

                    mystring += ";" + Environment.NewLine;
                }
            }

            return mystring;
        }

        public INodes GetAllHeadNodes()
        {
            List<INode> toNodes = new List<INode>();

            foreach (var edges in _adjacencyList.Values.ToList())
            {
                foreach (var edge in edges)
                {
                    toNodes.Add(edge.GetHeadNode());
                }
            }

            return new Nodes(toNodes);
        }

        // 
        // TODO: Switch this to iterative depth search (with dfs limit default set to max depth of given truck examples)
        ///
        /// <summary>
        ///     A depth-first-search (DFS) traversal of given tree
        /// </summary>
        /// <param name="graph">to traverse</param>
        /// <returns>
        ///    The List of the traversed nodes in exact order
        /// </returns>
        public INodes TraverseDepthFirst(Action<INode, INodes, INodes> action,
            CustomerOrderPart startNode)
        {
            var stack = new Stack<INode>();

            Dictionary<INode, bool> discovered = new Dictionary<INode, bool>();
            INodes traversed = new Nodes();

            stack.Push(startNode);
            INode parentNode;

            while (stack.Any())
            {
                INode poppedNode = stack.Pop();

                // init dict if node not yet exists
                if (!discovered.ContainsKey(poppedNode))
                {
                    discovered[poppedNode] = false;
                }

                // if node is not discovered
                if (!discovered[poppedNode])
                {
                    traversed.Add(poppedNode);
                    discovered[poppedNode] = true;
                    INodes childNodes = GetSuccessorNodes(poppedNode);
                    action(poppedNode, childNodes, traversed);

                    if (childNodes != null)
                    {
                        foreach (INode node in childNodes)
                        {
                            stack.Push(node);
                        }
                    }
                }
            }

            return traversed;
        }

        public INodes GetAllTailNodes()
        {
            return new Nodes(_adjacencyList.Keys.ToList());
        }

        public INodes GetAllUniqueNode()
        {
            INodes fromNodes = GetAllTailNodes();
            INodes toNodes = GetAllHeadNodes();
            IStackSet<INode> uniqueNodes = new StackSet<INode>();
            uniqueNodes.PushAll(fromNodes);
            uniqueNodes.PushAll(toNodes);

            return new Nodes(uniqueNodes.GetAll());
        }

        public GanttChart GetAsGanttChart(IDbTransactionData dbTransactionData)
        {
            GanttChart ganttChart = new GanttChart();

            foreach (var node in GetAllUniqueNode())
            {
                if (node.GetEntity().GetType() != typeof(ProductionOrderBom) &&
                    node.GetEntity().GetType() != typeof(ProductionOrderOperation) 
                    // && node.GetEntity().GetType() != typeof(PurchaseOrderPart)
                    )
                {
                    continue;
                }

                if (node.GetNodeType().Equals(NodeType.Demand))
                {
                    Demand demand = (Demand) node.GetEntity();
                    GanttChartBar ganttChartBar = new GanttChartBar()
                    {
                        article = demand.GetArticle().Name,
                        articleId = demand.GetArticle().Id.ToString(),
                        end = demand.GetDueTime(dbTransactionData).ToString(),
                    };
                    if (demand.GetStartTime(dbTransactionData) != null)
                    {
                        ganttChartBar.start = demand.GetStartTime(dbTransactionData).ToString();
                    }

                    if (demand.GetType() == typeof(ProductionOrderBom))
                    {
                        ProductionOrderBom productionOrderBom =
                            (ProductionOrderBom) demand.GetEntity();

                        ProductionOrderOperation productionOrderOperation =
                            productionOrderBom.GetProductionOrderOperation(dbTransactionData);
                        if (productionOrderOperation == null)
                        {
                            continue;
                        }

                        ganttChartBar.operation = productionOrderOperation.GetValue().Name;
                            ganttChartBar.operationId =
                                productionOrderOperation.GetValue().Id.ToString();
                            ganttChartBar.resource = productionOrderOperation.GetValue().MachineId.ToString();
                        
                    }

                    ganttChart.AddGanttChartBar(ganttChartBar);
                }
                else if (node.GetNodeType().Equals(NodeType.Provider))
                {
                    Provider provider = (Provider) node.GetEntity();
                    GanttChartBar ganttChartBar = new GanttChartBar()
                    {
                        article = provider.GetArticle().Name,
                        articleId = provider.GetArticle().Id.ToString(),
                        end = provider.GetDueTime(dbTransactionData).ToString()
                    };
                    if (provider.GetStartTime(dbTransactionData) != null)
                    {
                        ganttChartBar.start = provider.GetStartTime(dbTransactionData).ToString();
                    }

                    ganttChart.AddGanttChartBar(ganttChartBar);
                }
            }

            // TODO: remove this once forward scheduling is implemented
            int min = 0;
            foreach (var ganttChartBar in ganttChart.GetAllGanttChartBars())
            {
                if (ganttChartBar.start == null)
                {
                    ganttChartBar.start = ganttChartBar.end;
                }

                if (ganttChartBar.start != null)
                {
                    int start = int.Parse(ganttChartBar.start);
                    if (start < min)
                    {
                        min = start;
                    }
                }
            }

            if (min < 0)
            {
                foreach (var ganttChartBar in ganttChart.GetAllGanttChartBars())
                {
                    if (ganttChartBar.start != null)
                    {
                        int start = int.Parse(ganttChartBar.start);
                        ganttChartBar.start = (Math.Abs(min) + start).ToString();
                    }

                    if (ganttChartBar.end != null)
                    {
                        int end = int.Parse(ganttChartBar.end);
                        ganttChartBar.end = (Math.Abs(min) + end).ToString();
                    }
                }
            }

            return ganttChart;
        }

        public void RemoveNode(INode node)
        {
            List<IEdge> edgesTowardsNode = GetAllEdgesTowardsHeadNode(node);
            List<IEdge> edgesFromNode = GetAllEdgesFromTailNode(node);
            RemoveAllEdgesFromTailNode(node);
            RemoveAllEdgesTowardsHeadNode(node);

            // node is NOT start node AND NOT leaf node
            if (edgesTowardsNode != null && edgesFromNode != null)
            {
                foreach (var edgeTowardsNode in edgesTowardsNode)

                {
                    foreach (var edgeFromNode in edgesFromNode)
                    {
                        AddEdge(edgeTowardsNode.GetTailNode(),
                            new Edge(edgeTowardsNode.GetTailNode(), edgeFromNode.GetHeadNode()));
                    }
                }
            }

            _adjacencyList.Remove(node);
        }

        public void RemoveAllEdgesFromTailNode(INode tailNode)
        {
            _adjacencyList.Remove(tailNode);
        }

        public void RemoveAllEdgesTowardsHeadNode(INode headNode)
        {
            foreach (var tailNode in GetAllTailNodes())
            {
                List<IEdge> edgesToDelete = new List<IEdge>();
                foreach (var edge in _adjacencyList[tailNode])
                {
                    if (edge.GetHeadNode().Equals(headNode))
                    {
                        edgesToDelete.Add(edge);
                    }
                }

                foreach (var edgeToDelete in edgesToDelete)
                {
                    _adjacencyList[tailNode].Remove(edgeToDelete);
                }
            }
        }

        public INodes GetLeafNodes()
        {
            List<INode> leafs = new List<INode>();
            foreach (var uniqueNode in GetAllUniqueNode())
            {
                INodes successors = GetSuccessorNodes(uniqueNode);
                if (successors == null)
                {
                    leafs.Add(uniqueNode);
                }
            }

            if (leafs.Any() == false)
            {
                return null;
            }

            return new Nodes(leafs);
        }

        public INodes GetStartNodes()
        {
            List<INode> starts = new List<INode>();
            foreach (var uniqueNode in GetAllUniqueNode())
            {
                INodes predecessor = GetPredecessorNodes(uniqueNode);
                if (predecessor == null)
                {
                    starts.Add(uniqueNode);
                }
            }

            return new Nodes(starts);
        }

        public void ReplaceNodeByDirectedGraph(INode node)
        {
            throw new NotImplementedException();
        }

        public IDirectedGraph<INode> MergeDirectedGraphs(List<IDirectedGraph<INode>> directedGraphs)
        {
            IDirectedGraph<INode> mergedDirectedGraph = new DirectedGraph(_dbTransactionData);
            foreach (var directedGraph in directedGraphs)
            {
                foreach (var edge in directedGraph.GetAllEdges())
                {
                    mergedDirectedGraph.AddEdge(edge.GetTailNode(), edge);
                }
            }

            return mergedDirectedGraph;
        }

        public List<IEdge> GetAllEdges()
        {
            List<IEdge> allEdges = new List<IEdge>();
            foreach (var edgeList in _adjacencyList.Values)
            {
                allEdges.AddRange(edgeList);
            }

            return allEdges;
        }

        public Dictionary<INode, List<IEdge>> GetAdjacencyList()
        {
            return _adjacencyList;
        }
    }
}