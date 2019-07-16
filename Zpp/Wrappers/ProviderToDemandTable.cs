using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
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

        public void AddAll(IProviderToDemandsMap providerToDemandsMap)
        {
            _providerToDemandEntities.AddRange(providerToDemandsMap.ToT_ProviderToDemand());
        }

        public List<T_ProviderToDemand> GetAll()
        {
            return _providerToDemandEntities;
        }

        public IProviderToDemandsMap ToProviderToDemands(IDbTransactionData dbTransactionData)
        {
            IProviderToDemandsMap providerToDemandsMap = new ProviderToDemandsMap();
            
            foreach (var demandToProviderEntity in _providerToDemandEntities)
            {
                Demand demand =
                    dbTransactionData.DemandsGetById(new Id(demandToProviderEntity.DemandId));
                Provider provider =
                    dbTransactionData.ProvidersGetById(new Id(demandToProviderEntity.ProviderId));
                if (demand == null || provider == null)
                {
                    throw new MrpRunException("Could not find demand or provider.");
                }
                
                providerToDemandsMap.AddDemandForProvider(provider, demand);
                    
            }

            return providerToDemandsMap;
        }

        public int Count()
        {
            return _providerToDemandEntities.Count;
        }
    }
}