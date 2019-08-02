using Master40.DB.DataModel;

namespace Zpp
{
    public class Edge : IEdge
    {
        private readonly T_DemandToProvider _demandToProvider;
        private readonly INode _fromNode;
        private readonly INode _toToNode;

        public Edge(T_DemandToProvider demandToProvider, INode fromNode, INode toNode)
        {
            _demandToProvider = demandToProvider;
            _fromNode = fromNode;
            _toToNode = toNode;
        }

        public INode GetFromNode()
        {
            return _fromNode;
        }
        
        public INode GetToNode()
        {
            return _toToNode;
        }

        public T_DemandToProvider GetDemandToProvider()
        {
            return _demandToProvider;
        }
    }
}