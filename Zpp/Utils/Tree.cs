using System.Collections.Generic;

namespace Zpp.Utils
{
    public abstract class Tree<TEntity>: ITree<TEntity>
    {
        private int counter = 0;
        
        protected Tree()
        {
            
        }
        
        public abstract List< Node<TEntity>> GetChildNodes( Node<TEntity> node);
        public abstract  Node<TEntity> GetRootNode();

        public abstract AdjacencyList< Node<TEntity>> GetAdjacencyList();

        protected Node<TEntity> createNode(TEntity entity)
        {
            return new Node<TEntity>(counter++, entity);
        }
    }
}