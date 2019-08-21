using System;
using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.GraphicalRepresentation;
using Zpp.Utils;

namespace Zpp
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
        INodes GetSuccessorNodes(TNode tailNode);
        
        INodes GetPredecessorNodes(TNode headNode);

        void AddEdges(TNode fromNode, List<IEdge> edges);
        
        void AddEdge(TNode fromNode, IEdge edge);

        int CountEdges();

        INodes GetAllHeadNodes();
        
        INodes GetAllTailNodes();

        /**
         * No duplicates should be contained
         */
        INodes GetAllUniqueNode();

        List<IEdge> GetAllEdgesFromTailNode(TNode tailNode);
        
        List<IEdge> GetAllEdgesTowardsHeadNode(TNode headNode);
        
        INodes TraverseDepthFirst(Action<TNode, INodes, INodes> action, CustomerOrderPart startNode);

        GanttChart GetAsGanttChart(IDbTransactionData dbTransactionData);

        /**
         * This removed the node, the edges towards it will point to its childs afterwards
         */
        void RemoveNode(TNode node);

        void RemoveAllEdgesFromTailNode(TNode tailNode);

        void RemoveAllEdgesTowardsHeadNode(TNode headNode);

        INodes GetLeafNodes();

        INodes GetStartNodes();

        void ReplaceNodeByDirectedGraph(TNode node);

        List<IEdge> GetAllEdges();

        Dictionary<TNode, List<IEdge>> GetAdjacencyList();
        
        
    }
}