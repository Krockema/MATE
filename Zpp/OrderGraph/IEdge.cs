using Master40.DB.DataModel;

namespace Zpp.OrderGraph
{
    public interface IEdge
    {
        INode GetTailNode();

        INode GetHeadNode();

        T_DemandToProvider GetDemandToProvider();
    }
}