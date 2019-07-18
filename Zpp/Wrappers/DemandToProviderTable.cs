using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    /**
     * wraps T_DemandToProvider
     */
    public class DemandToProviderTable : IDemandToProviderTable
    {
        private readonly List<T_DemandToProvider> _demandToProviderEntities =
            new List<T_DemandToProvider>();

        public DemandToProviderTable(List<T_DemandToProvider> demandToProviderEntities)
        {
            _demandToProviderEntities = demandToProviderEntities;
        }

        public DemandToProviderTable()
        {
        }

        public DemandToProviderTable(IDemandToProvidersMap demandToProvidersMap)
        {
            _demandToProviderEntities.AddRange(demandToProvidersMap.ToT_DemandToProvider());
        }

        public List<T_DemandToProvider> GetAll()
        {
            return _demandToProviderEntities;
        }

        public void AddAll(IDemandToProvidersMap demandToProvidersMap)
        {
            _demandToProviderEntities.AddRange(demandToProvidersMap.ToT_DemandToProvider());
        }

        public IDemandToProvidersMap ToDemandToProvidersMap(IDbTransactionData dbTransactionData)
        {
            IDemandToProvidersMap demandToProvidersMap = new DemandToProvidersMap();

            foreach (var demandToProviderEntity in _demandToProviderEntities)
            {
                Demand demand =
                    dbTransactionData.DemandsGetById(demandToProviderEntity.GetDemandId());
                Provider provider =
                    dbTransactionData.ProvidersGetById(demandToProviderEntity.GetProviderId());
                if (demand == null || provider == null)
                {
                    throw new MrpRunException(
                        $"Could not find demand ({demand}) or provider ({provider}) of demandToProvider" +
                        $"({demandToProviderEntity.GetDemandId()}->{demandToProviderEntity.GetProviderId()}).");
                }

                demandToProvidersMap.AddProviderForDemand(demand, provider);
            }

            return demandToProvidersMap;
        }

        public int Count()
        {
            return _demandToProviderEntities.Count;
        }
    }
}