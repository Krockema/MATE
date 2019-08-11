using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    public interface IProvidingManager
    {
        Response Satisfy(Demand demand, Quantity demandedQuantity, IDbTransactionData dbTransactionData);
    }
}