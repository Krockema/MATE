using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DbCache;

namespace Zpp.OrderGraph
{
    public interface INode
    {
        Id GetId();
        
        NodeType GetNodeType();

        INode GetEntity();

        string GetGraphizString(IDbTransactionData dbTransactionData);


    }
}