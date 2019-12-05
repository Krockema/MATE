using Master40.DB.DataModel;
using Zpp.Common.DemandDomain;
using Zpp.Common.ProviderDomain;

namespace Zpp.WrappersForCollections
{
    public interface IDemandToProviderTable: ICollectionWrapper<T_DemandToProvider>
    {
        bool Contains(Demand demand);
        
        bool Contains(Provider provider);
    }
}