namespace Zpp.Utils
{
    /// <summary>
    /// Represents the bomEdge + childNode:
    /// e.g. A---1--->B where A is parentArticle, 1 is articleBom, B is childArticle, Node would represent 1 and B
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class Node<TNode>
    {
        private readonly int _bomId;
        private readonly TNode _entity;

        public Node(int bomId, TNode entity)
        {
            _bomId = bomId;
            _entity = entity;
        }

        public int BomId => _bomId;

        public TNode Entity => _entity;
        
        public override string ToString()
        {
            return _bomId.ToString();
        }
    }
    
    
}