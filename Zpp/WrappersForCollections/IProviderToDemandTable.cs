using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.ProviderDomain;

namespace Zpp.WrappersForCollections
{
    public interface IProviderToDemandTable: ICollectionWrapper<T_ProviderToDemand>
    {
        void Add(Provider provider, Id demandId, Quantity quantity);
    }
}