using System.Collections.Generic;
using Zpp.Utils;

namespace Zpp
{
    /**
     * NOTE: TNode is just a representation of a node, it can occur multiple time
     * and is not a unique runtime object, but equal should return true.
     */
    public interface IGraph<TNode>
    {
        List<TNode> GetToNodesOfFromNode(TNode fromNode);

        void AddEdges(TNode fromNode, List<TNode> toNodes);
        
        void AddEdge(TNode fromNode, TNode toNode);

        int CountEdges();

        List<INode> GetAllToNodes();
    }
}