using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

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

        public List<T_DemandToProvider> GetAll()
        {
            return _demandToProviderEntities;
        }

        public void AddAll(IDemandToProviderTable demandToProviderTable)
        {
            _demandToProviderEntities.AddRange(demandToProviderTable.GetAll());
        }

        public int Count()
        {
            return _demandToProviderEntities.Count;
        }

        public void Add(T_DemandToProvider demandToProvider)
        {
            _demandToProviderEntities.Add(demandToProvider);
        }

        public bool Contains(Demand demand)
        {
            return _demandToProviderEntities.Select(x => x.DemandId).ToList()
                .Contains(demand.GetId().GetValue());
        }

        public bool Contains(Provider provider)
        {
            return _demandToProviderEntities.Select(x => x.ProviderId).ToList()
                .Contains(provider.GetId().GetValue());
        }
    }
}