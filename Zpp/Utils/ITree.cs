
using System.Collections.Generic;
using Zpp.Utils;

namespace Zpp.Utils
{
    public interface ITree<TEntity>
    {
        List< Node<TEntity>> GetChildNodes( Node<TEntity> node);
        Node<TEntity> GetRootNode();

        AdjacencyList<TEntity> GetAdjacencyList();
    }
}