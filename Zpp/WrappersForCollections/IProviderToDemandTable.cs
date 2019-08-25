using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.ProviderDomain;

namespace Zpp
{
    public interface IProviderToDemandTable: ICollectionWrapper<T_ProviderToDemand>
    {
        void Add(Provider provider, Id demandId);
    }
}