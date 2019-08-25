using Master40.DB.DataModel;

namespace Zpp
{
    public interface IEdge
    {
        INode GetTailNode();

        INode GetHeadNode();

        T_DemandToProvider GetDemandToProvider();
    }
}