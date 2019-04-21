using System;

namespace Zpp.Utils
{
    /// <summary>
    /// Represents the bomEdge + childNode:
    /// e.g. A---1--->B where A is parentArticle, 1 is articleBom, B is childArticle, Node would represent 1 and B
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class Node<TNode> : IComparable<Node<TNode>>
    {
        private readonly int _bomId;
        private readonly TNode _entity;
        private readonly int _id;

        public Node(int id, int bomId, TNode entity)
        {
            _bomId = bomId;
            _entity = entity;
        }

        public int BomId => _bomId;

        public int Id => _id;

        public TNode Entity => _entity;
        
        public override string ToString()
        {
            return _bomId.ToString();
        }
        
        public int CompareTo(Node<TNode> that)
        {
            if (this._id >  that.Id) return -1;
            if (this._id == that.Id) return 0;
            return 1;
        }
    }
    
    
}