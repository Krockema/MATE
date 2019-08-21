using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp
{
    public interface INode
    {
        Id GetId();
        
        NodeType GetNodeType();

        INode GetEntity();

        string GetGraphizString(IDbTransactionData dbTransactionData);
        
        string GetJsonString(IDbTransactionData dbTransactionData);
        
        
    }
}