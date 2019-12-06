using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore.Internal;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.GraphicalRepresentation;
using Zpp.GraphicalRepresentation.impl;
using Zpp.Util.StackSet;

namespace Zpp.Util.Graph.impl
{
    /**
     * An impl for a directed graph. It's important to always return null if aggregation is empty
     * (simplify error detecting, since no empty collections should pass through the program).
     * A directed graph is stored/read in by its edges, from that edges a list of nodes is built up
     * where every node has:
     * - a list of successors
     * - a list of predecessors
     * --> so that O(1) can be realized for most operations
     */
    public class DirectedGraph : IDirectedGraph<INode>
    {
        protected IStackSet<IGraphNode> _nodes = new StackSet<IGraphNode>();

        protected readonly IGraphviz Graphviz = new Graphviz();

        public DirectedGraph()
        {
        }

        public DirectedGraph(List<IEdge> edges)
        {
            AddEdges(edges);
        }

        public INodes GetSuccessorNodes(Id nodeId)
        {
            IGraphNode graphNode = _nodes.GetById(nodeId);
            if (graphNode == null)
            {
                // node doesn't exists
                throw new MrpRunException($"Given node ({graphNode}) doesn't exists in graph.");
            }

            GraphNodes successors = graphNode.GetSuccessors();

            if (successors.Any() == false)
            {
                return null;
            }

            return new Nodes(successors.Select(x => x.GetNode()));
        }

        public INodes GetSuccessorNodes(INode node)
        {
            return GetSuccessorNodes(node.GetId());
        }

        public INodes GetPredecessorNodes(INode node)
        {
            return GetPredecessorNodes(node.GetId());
        }

        public INodes GetPredecessorNodesRecursive(INode startNode)
        {
            INodes allPredecessors = new Nodes();
            Stack<INode> stack = new Stack<INode>();
            stack.Push(startNode);

            while (stack.Any())
            {
                INode poppedNode = stack.Pop();
                if (poppedNode.Equals(startNode) == false)
                {
                    allPredecessors.Add(poppedNode);   
                }

                INodes predecessors = GetPredecessorNodes(poppedNode.GetId());
                if (predecessors != null)
                {
                    foreach (var predecessor in predecessors)
                    {
                        stack.Push(predecessor);
                    }
                }
               
            }

            if (allPredecessors.Any() == false)
            {
                return null;
            }
            return allPredecessors;
        }
        
        public INodes GetPredecessorNodes(Id nodeId)
        {
            IGraphNode graphNode = _nodes.GetById(nodeId);
            if (graphNode == null)
            {
                // node doesn't exists
                throw new MrpRunException($"Given node ({graphNode}) doesn't exists in graph.");
            }

            GraphNodes predecessors = graphNode.GetPredecessors();

            if (predecessors.Any() == false)
            {
                return null;
            }

            return new Nodes(predecessors.Select(x => x.GetNode()));
        }

        public void AddEdges(IEnumerable<IEdge> edges)
        {
            foreach (var edge in edges)
            {
                AddEdge(edge);
            }
        }

        public void AddEdges(INode fromNode, INodes nodes)
        {
            foreach (var toNode in nodes)
            {
                AddEdge(new Edge(fromNode, toNode));
            }
        }

        public void AddEdge(IEdge edge)
        {
            INode edgeTail = edge.TailNode;
            INode edgeHead = edge.HeadNode;
            IGraphNode tail;
            IGraphNode head;
            if (_nodes.Contains(edgeTail.GetId()) == false)
            {
                tail = new GraphNode(edgeTail);
                _nodes.Push(tail);
            }
            else
            {
                tail = _nodes.GetById(edgeTail.GetId());
            }

            if (_nodes.Contains(edgeHead.GetId()) == false)
            {
                head = new GraphNode(edgeHead);
                _nodes.Push(head);
            }
            else
            {
                head = _nodes.GetById(edgeHead.GetId());
            }

            head.AddPredecessor(tail);
            tail.AddSuccessor(head);
        }

        public int CountEdges()
        {
            return _nodes.Count();
        }

        public override string ToString()
        {
            string mystring = "";
            IStackSet<IEdge> edges = GetEdges();

            if (edges == null)
            {
                return mystring;
            }

            foreach (var edge in edges)
            {
                string tailsGraphvizString =
                    Graphviz.GetGraphizString(edge.GetTailNode().GetEntity());
                string headsGraphvizString =
                    Graphviz.GetGraphizString(edge.GetHeadNode().GetEntity());
                mystring += $"\"{tailsGraphvizString}\" -> " + $"\"{headsGraphvizString}\"";

                mystring += ";" + Environment.NewLine;
                // }
            }

            return mystring;
        }

        /// 
        ///
        /// <summary>
        ///     A depth-first-search (DFS) traversal of given tree
        /// </summary>
        /// <returns>
        ///    The List of the traversed nodes in exact order
        /// </returns>
        public INodes TraverseDepthFirst(Action<INode, INodes, INodes> action,
            CustomerOrderPart startNode)
        {
            var stack = new Stack<INode>();

            Dictionary<INode, bool> discovered = new Dictionary<INode, bool>();
            INodes traversed = new Nodes();

            stack.Push(new Node(startNode));
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

        public IStackSet<INode> GetAllUniqueNodes()
        {
            IStackSet<INode> uniqueNodes = new StackSet<INode>();
            uniqueNodes.PushAll(_nodes.Select(x => x.GetNode()));

            if (uniqueNodes.Any() == false)
            {
                return null;
            }

            return uniqueNodes;
        }

        public bool Contains(INode node)
        {
            return Contains(node.GetId());
        }

        public bool Contains(Id nodeId)
        {
            IGraphNode graphNode = _nodes.GetById(nodeId);
            return graphNode != null;
        }

        public void RemoveNode(INode node, bool connectParentsWithChilds, bool removeEdges = true)
        {
            RemoveNode(node.GetId(), connectParentsWithChilds, removeEdges);
        }

        public void RemoveNode(Id nodeId, bool connectParentsWithChilds, bool removeEdges = true)
        {
            // e.g. A -> B --> C, B is removed

            IGraphNode graphNode = _nodes.GetById(nodeId);
            if (graphNode == null)
            {
                // node doesn't exists
                throw new MrpRunException($"Given node ({graphNode}) doesn't exists in graph.");
            }

            // holds A
            GraphNodes predecessors = graphNode.GetPredecessors();
            // holds C
            GraphNodes successors = graphNode.GetSuccessors();

            if (connectParentsWithChilds)
            {
                foreach (var predecessor in predecessors)
                {
                    // predecessor is A

                    // remove edge A -> B
                    predecessor.RemoveSuccessor(graphNode);
                    // add edge A -> C
                    predecessor.AddSuccessors(successors);
                }

                foreach (var successor in successors)
                {
                    // successor is C

                    // remove edge B -> C
                    successor.RemovePredecessor(graphNode);
                    // add edge A -> C
                    successor.AddPredecessors(predecessors);
                }
            }
            else if (removeEdges)
            {
                foreach (var predecessor in predecessors)
                {
                    // predecessor is A

                    // remove edge A -> B
                    predecessor.RemoveSuccessor(graphNode);
                }

                foreach (var successor in successors)
                {
                    // successor is C

                    // remove edge B -> C
                    successor.RemovePredecessor(graphNode);
                }
            }

            // remove node
            _nodes.Remove(graphNode);
        }

        public INodes GetLeafNodes()
        {
            INodes leafs = new Nodes();

            foreach (var node in _nodes)
            {
                INodes successors = GetSuccessorNodes(node.GetNode());
                if (successors == null || successors.Any() == false)
                {
                    leafs.Add(node.GetNode());
                }
            }

            if (leafs.Any() == false)
            {
                return null;
            }

            return leafs;
        }

        public bool IsEmpty()
        {
            return _nodes == null || _nodes.Any() == false;
        }

        public INodes GetRootNodes()
        {
            INodes roots = new Nodes();

            foreach (var node in _nodes)
            {
                INodes predecessors = GetPredecessorNodes(node.GetNode());
                if (predecessors == null)
                {
                    roots.Add(node.GetNode());
                }
            }

            if (roots.Any() == false)
            {
                return null;
            }

            return roots;
        }

        public void ReplaceNodeByDirectedGraph(INode node, IDirectedGraph<INode> graphToInsert)
        {
            ReplaceNodeByDirectedGraph(node.GetId(), graphToInsert);
        }

        public void ReplaceNodeByOtherNode(Id nodeId, INode otherNode)
        {
            if (Contains(otherNode))
            {
                otherNode = GetNode(otherNode.GetId());
            }
            INodes predecessors = GetPredecessorNodes(nodeId);
            INodes successors = GetSuccessorNodes(nodeId);
            RemoveNode(nodeId, false);
            // predecessors --> roots
            if (predecessors != null)
            {
                foreach (var predecessor in predecessors)
                {

                        AddEdge(new Edge(predecessor, otherNode));

                }
            }

            // leafs --> successors 
            if (successors != null)
            {

                    foreach (var successor in successors)
                    {
                        AddEdge(new Edge(otherNode, successor));
                    }
            }
            
        }

        public void ReplaceNodeByDirectedGraph(Id nodeId, IDirectedGraph<INode> graphToInsert)
        {
            INodes predecessors = GetPredecessorNodes(nodeId);
            INodes successors = GetSuccessorNodes(nodeId);
            RemoveNode(nodeId, false);
            // predecessors --> roots
            if (predecessors != null)
            {
                foreach (var predecessor in predecessors)
                {
                    foreach (var rootNode in graphToInsert.GetRootNodes())
                    {
                        AddEdge(new Edge(predecessor, rootNode));
                    }
                }
            }

            // leafs --> successors 
            if (successors != null)
            {
                foreach (var leaf in graphToInsert.GetLeafNodes())
                {
                    foreach (var successor in successors)
                    {
                        AddEdge(new Edge(leaf, successor));
                    }
                }
            }

            // add all edges from graphToInsert
            AddEdges(graphToInsert.GetEdges());
        }

        public INode GetNode(Id id)
        {
            IGraphNode graphNode = _nodes.GetById(id); 
            if (graphNode == null)
            {
                throw new MrpRunException($"A with id {id} doesn't exist.");
            }
            return graphNode.GetNode();
        }

        private ILinkDemandAndProvider GetDemandOrProviderLink(INode tailGraphNode,
            INode headGraphNode)
        {
            ILinkDemandAndProvider demandAndProviderLink;
            IScheduleNode tail = tailGraphNode.GetEntity();
            if (tail is Demand)
            {
                demandAndProviderLink = new T_DemandToProvider(tail.GetId(),
                    headGraphNode.GetEntity().GetId(), null);
            }
            else if (tail is Provider)
            {
                demandAndProviderLink = new T_ProviderToDemand(tail.GetId(),
                    headGraphNode.GetEntity().GetId(), null);
            }
            else
            {
                throw new MrpRunException("Not expected type.");
            }

            return demandAndProviderLink;
        }

        public List<ILinkDemandAndProvider> GetEdgesTo(Id nodeId)
        {
            List<ILinkDemandAndProvider> demandAndProviders = new List<ILinkDemandAndProvider>();
            INode node = GetNode(nodeId);
            INodes predecessors = GetPredecessorNodes(nodeId);

            foreach (var predecessor in predecessors)
            {
                ILinkDemandAndProvider demandAndProviderLink =
                    GetDemandOrProviderLink(predecessor, node);
                demandAndProviders.Add(demandAndProviderLink);
            }

            return demandAndProviders;
        }

        public List<ILinkDemandAndProvider> GetEdgesFrom(Id nodeId)
        {
            List<ILinkDemandAndProvider> demandAndProviders = new List<ILinkDemandAndProvider>();
            INode node = GetNode(nodeId);
            INodes successors = GetSuccessorNodes(nodeId);
            
            foreach (var successor in successors)
            {
                ILinkDemandAndProvider demandAndProviderLink =
                    GetDemandOrProviderLink(node, successor);
                demandAndProviders.Add(demandAndProviderLink);
            }

            return demandAndProviders;
        }

        public List<ILinkDemandAndProvider> GetEdgesOn(Id nodeId)
        {
            List<ILinkDemandAndProvider> demandAndProviders = new List<ILinkDemandAndProvider>();
            demandAndProviders.AddRange(GetEdgesFrom(nodeId));
            demandAndProviders.AddRange(GetEdgesTo(nodeId));

            return demandAndProviders;
        }

        public IStackSet<IEdge> GetEdges()
        {
            IStackSet<IEdge> edges = new StackSet<IEdge>();

            // one is enough either all successors or all predecessors
            foreach (var node in _nodes)
            {
                foreach (var successor in node.GetSuccessors())
                {
                    edges.Push(new Edge(node.GetNode(), successor.GetNode()));
                }
            }

            if (edges.Any() == false)
            {
                return null;
            }

            return edges;
        }

        public void Clear()
        {
            _nodes.Clear();
        }

        public IStackSet<IGraphNode> GetNodes()
        {
            return _nodes;
        }

        public void RemoveEdge(INode parent, INode child)
        {
            IGraphNode parentGraphNode = _nodes.GetById(parent.GetId());
            IGraphNode childGraphNode = _nodes.GetById(child.GetId());
            parentGraphNode.RemoveSuccessor(childGraphNode);
            childGraphNode.RemovePredecessor(parentGraphNode);
        }
    }
}