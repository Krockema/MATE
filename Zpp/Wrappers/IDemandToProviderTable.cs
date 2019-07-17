using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandToProviderDomain;

namespace Zpp
{
    public interface IDemandToProviderTable
    {
        void AddAll(IDemandToProvidersMap demandToProvidersMap);

        List<T_DemandToProvider> GetAll();

        IDemandToProvidersMap ToDemandToProvidersMap(IDbTransactionData dbTransactionData);

        int Count();
    }
}