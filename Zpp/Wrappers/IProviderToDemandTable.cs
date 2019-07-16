using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandToProviderDomain;

namespace Zpp
{
    public interface IProviderToDemandTable
    {
        void AddAll(IProviderToDemandsMap providerToDemandsMap);

        List<T_ProviderToDemand> GetAll();

        IProviderToDemandsMap ToProviderToDemands(IDbTransactionData dbTransactionData);

        int Count();
    }
}