using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp
{
    public class Node : INode
    {
        private INode _entity;
        private List<INode> _childEntities;

        public Node(INode entity)
        {
            _entity = entity;
        }

        public Id GetId()
        {
            return _entity.GetId();
        }

        public NodeType GetNodeType()
        {
            return _entity.GetNodeType();
        }

        public INode GetEntity()
        {
            return _entity;
        }

        public void AddChild(Node node)
        {
            _childEntities.Add(node);
        }

        public List<INode> GetChilds()
        {
            return _childEntities;
        }
    }
}