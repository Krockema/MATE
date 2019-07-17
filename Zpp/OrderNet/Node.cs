using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp
{
    public class Node : INode
    {
        private Id _id;
        private INode _entity;
        private List<INode> _childEntities;

        public Node(INode entity, Id id)
        {
            _entity = entity;
            _id = id;
        }

        public Id GetId()
        {
            return _id;
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

        public override bool Equals(object obj)
        {
            INode otherObject = (INode) obj;
            // return _id.Equals(otherObject.GetId()) && _entity.GetNodeType().Equals(otherObject.GetNodeType());
            return _id.Equals(otherObject.GetId());
        }

        public override int GetHashCode()
        {
            // return HashCode.Combine(_id.GetHashCode(), _entity.GetEntity().GetNodeType().GetHashCode());
            return _id.GetHashCode();
        }

        public string GetGraphizString()
        {
            return _entity.GetGraphizString();
        }
    }
}