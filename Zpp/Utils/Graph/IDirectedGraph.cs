using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.Util.Graph.impl;
using Zpp.Util.StackSet;

namespace Zpp.Util.Graph
{
    /**
     * NOTE: TNode is just a representation of a node, it can occur multiple time
     * and is not a unique runtime object, but equal should return true.
     */
    // TODO: rename From --> Tail, To --> Head
    public interface IDirectedGraph<TNode>
    {
        /**
         * one fromNode has many toNodes
         * @return: toNodes
         */
        INodes GetSuccessorNodes(TNode node);
        
        INodes GetSuccessorNodes(Id nodeId);

        INodes GetPredecessorNodes(INode node);
        
        INodes GetPredecessorNodes(Id nodeId);

        INodes GetPredecessorNodesRecursive(INode startNode);

        void AddEdges(IEnumerable<IEdge> edges);

        void AddEdges(TNode fromNode, INodes nodes);

        void AddEdge(IEdge edge);

        int CountEdges();

        /**
         * No duplicates should be contained
         */
        IStackSet<INode> GetAllUniqueNodes();

        INodes TraverseDepthFirst(Action<TNode, INodes, INodes> action,
            CustomerOrderPart startNode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="connectParentsWithChilds"> if true this removes the node,
        /// the parents will point to its childs afterwards</param>
        /// /// <param name="removeEdges">take false if all predecessors/successors
        /// are also removed</param>
        void RemoveNode(TNode node, bool connectParentsWithChilds, bool removeEdges = true);
        
        void RemoveNode(Id nodeId, bool connectParentsWithChilds, bool removeEdges = true);

        INodes GetLeafNodes();

        INodes GetRootNodes();

        void ReplaceNodeByDirectedGraph(TNode node, IDirectedGraph<INode> graphToInsert);
        
        void ReplaceNodeByDirectedGraph(Id nodeId, IDirectedGraph<INode> graphToInsert);
        
        void ReplaceNodeByOtherNode(Id nodeId, INode otherNode);

        IStackSet<IEdge> GetEdges();
        
        List<ILinkDemandAndProvider> GetEdgesOn(Id nodeId);
        
        List<ILinkDemandAndProvider> GetEdgesTo(Id nodeId);
        
        List<ILinkDemandAndProvider> GetEdgesFrom(Id nodeId);

        void Clear();

        bool IsEmpty();

        bool Contains(INode node);
        
        bool Contains(Id nodeId);

        INode GetNode(Id id);

        IStackSet<IGraphNode> GetNodes();

        void RemoveEdge(INode parent, INode child);
    }
}