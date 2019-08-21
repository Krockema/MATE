using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp
{
    public class Node : INode
    {
        private Id _id;
        private INode _entity;

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

        public string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            return _entity.GetGraphizString(dbTransactionData);
        }

        public string GetJsonString(IDbTransactionData dbTransactionData)
        {
            return _entity.GetJsonString(dbTransactionData);
        }

        public override string ToString()
        {
            return $"{_entity.ToString()}";
        }
    }
}