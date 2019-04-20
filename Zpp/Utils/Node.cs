namespace Zpp.Utils
{
    public class Node<TNode>
    {
        private int _id;
        private readonly TNode _entity;

        public Node(int id, TNode entity)
        {
            _id = id;
            _entity = entity;
        }

        public int Id => _id;

        public TNode Entity => _entity;
    }
}