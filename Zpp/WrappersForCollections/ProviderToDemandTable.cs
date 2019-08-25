using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class ProviderToDemandTable : CollectionWrapperWithList<T_ProviderToDemand>, IProviderToDemandTable
    {
        public ProviderToDemandTable(List<T_ProviderToDemand> list) : base(list)
        {
        }

        public ProviderToDemandTable()
        {
        }

        public void Add(Provider provider, Id demandId)
        {
            T_ProviderToDemand providerToDemand = new T_ProviderToDemand();
            providerToDemand.DemandId = demandId.GetValue();
            providerToDemand.ProviderId = provider.GetId().GetValue();

            List.Add(providerToDemand);
        }
        
    }
}