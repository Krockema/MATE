using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    public interface IOpenDemandManager
    {
        void AddDemand(Id providerId, Demand oneDemand, Quantity reservedQuantity);

        ResponseWithDemands SatisfyProviderByOpenDemand(Provider provider, Quantity demandedQuantity,
            IDbTransactionData dbTransactionData);
    }
}