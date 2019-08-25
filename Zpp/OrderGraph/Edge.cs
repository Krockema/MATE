using Master40.DB.DataModel;

namespace Zpp
{
    public class Edge : IEdge
    {
        private readonly T_DemandToProvider _demandToProvider;
        private readonly INode _tailNode;
        private readonly INode _headNode;

        public Edge(T_DemandToProvider demandToProvider, INode tailNode, INode toNode)
        {
            _demandToProvider = demandToProvider;
            _tailNode = tailNode;
            _headNode = toNode;
        }
        
        public Edge(INode tailNode, INode toNode)
        {
            _tailNode = tailNode;
            _headNode = toNode;
        }

        public INode GetTailNode()
        {
            return _tailNode;
        }
        
        public INode GetHeadNode()
        {
            return _headNode;
        }

        public T_DemandToProvider GetDemandToProvider()
        {
            return _demandToProvider;
        }

        public override string ToString()
        {
            return $"{_tailNode} --> {_headNode}";
        }
    }
}