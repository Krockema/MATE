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
        List<TNode> GetChildNodes(TNode node);
        Dictionary<TNode, List<TNode>> GetAdjacencyList();

        void AddChilds(INode node, List<INode> nodes);
        
        void AddChild(INode node, INode childNode);
    }
}