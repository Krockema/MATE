using System;
using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.Utils;

namespace Zpp
{
    /**
     * NOTE: TNode is just a representation of a node, it can occur multiple time
     * and is not a unique runtime object, but equal should return true.
     */
    public interface IGraph<TNode>
    {
        /**
         * one fromNode has many toNodes
         * @return: toNodes
         */
        List<TNode> GetChildNodes(TNode fromNode);

        void AddEdges(TNode fromNode, List<IEdge> edges);
        
        void AddEdge(TNode fromNode, IEdge edge);

        int CountEdges();

        List<INode> GetAllToNodes();
        
        List<INode> TraverseDepthFirst(Action<INode, List<INode>> action, CustomerOrderPart startNode);
    }
}