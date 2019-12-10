using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.Util.Graph.impl
{
    public class Edge : IEdge
    {
        private readonly ILinkDemandAndProvider _demandToProvider;
        public INode TailNode { get; }
        public INode HeadNode { get; }
        private readonly Id _id = IdGeneratorHolder.GetIdGenerator().GetNewId();

        public Edge(ILinkDemandAndProvider demandToProvider, INode tailNode, INode toNode)
        {
            _demandToProvider = demandToProvider;
            TailNode = tailNode;
            HeadNode = toNode;
        }

        public Edge(INode tailNode, INode toNode)
        {
            TailNode = tailNode;
            HeadNode = toNode;
        }

        public INode GetTailNode()
        {
            return TailNode;
        }

        public INode GetHeadNode()
        {
            return HeadNode;
        }

        public ILinkDemandAndProvider GetLinkDemandAndProvider()
        {
            return _demandToProvider;
        }

        public override string ToString()
        {
            return $"{TailNode} --> {HeadNode}";
        }

        public override bool Equals(object obj)
        {
            Edge other = (Edge) obj;
            bool headAndTailAreEqual =
                HeadNode.Equals(other.HeadNode) && TailNode.Equals(other.TailNode);
            if (_demandToProvider == null)
            {
                return headAndTailAreEqual && _demandToProvider == other._demandToProvider;
            }
            else
            {
                return headAndTailAreEqual && _demandToProvider.Equals(other._demandToProvider);
            }
        }

        public override int GetHashCode()
        {
            return HeadNode.GetHashCode() + TailNode.GetHashCode();
        }

        public Id GetId()
        {
            return _id;
        }
    }
}