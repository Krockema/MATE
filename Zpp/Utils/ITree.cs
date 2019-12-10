using System.Collections.Generic;
using Zpp.Utils;

namespace Zpp.Utils
{
    /**
     * As long as the entity only represents itself with an id, this could be used as graph.
     * The cause is that in a graph a node A can have a child C while a node B can also have the child c.
     * C occurs two times and is only equal by id, not the same runtime object ! 
     */
    public interface ITree<TEntity>
    {
        List<Node<TEntity>> GetChildNodes(Node<TEntity> node);
        Node<TEntity> GetRootNode();

        AdjacencyList<TEntity> GetAdjacencyList();
    }
}