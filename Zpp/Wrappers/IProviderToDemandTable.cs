using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.ProviderDomain;

namespace Zpp
{
    public interface IProviderToDemandTable
    {
        void Add(Provider provider, Id demandId);

        List<T_ProviderToDemand> GetAll();

        int Count();
    }
}