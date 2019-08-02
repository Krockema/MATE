using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class ProviderToDemandTable : IProviderToDemandTable
    {
        private readonly List<T_ProviderToDemand> _providerToDemandEntities = new List<T_ProviderToDemand>();

        public ProviderToDemandTable()
        {
        }

        public ProviderToDemandTable(List<T_ProviderToDemand> providerToDemandEntities)
        {
            _providerToDemandEntities = providerToDemandEntities;
        }

        public List<T_ProviderToDemand> GetAll()
        {
            return _providerToDemandEntities;
        }

        public int Count()
        {
            return _providerToDemandEntities.Count;
        }

        public void Add(Provider provider, Id demandId)
        {
            T_ProviderToDemand providerToDemand = new T_ProviderToDemand();
            providerToDemand.DemandId = demandId.GetValue();
            providerToDemand.ProviderId = provider.GetId().GetValue();
            
            _providerToDemandEntities.Add(providerToDemand);
        }
    }
}