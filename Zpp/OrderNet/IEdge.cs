using Master40.DB.DataModel;

namespace Zpp
{
    public interface IEdge
    {
        INode GetFromNode();

        INode GetToNode();

        T_DemandToProvider GetDemandToProvider();
    }
}