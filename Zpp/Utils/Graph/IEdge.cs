using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.Util.Graph
{
    public interface IEdge: IId
    {
        INode TailNode { get;  }
        
        INode HeadNode { get;  }
        
        INode GetTailNode();

        INode GetHeadNode();

        ILinkDemandAndProvider GetLinkDemandAndProvider();
    }
}