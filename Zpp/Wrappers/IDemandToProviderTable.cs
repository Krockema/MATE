using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public interface IDemandToProviderTable: ICollectionWrapper<T_DemandToProvider>
    {
        bool Contains(Demand demand);
        
        bool Contains(Provider provider);
    }
}