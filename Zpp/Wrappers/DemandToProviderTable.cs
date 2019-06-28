using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandToProviderDomain;

namespace Zpp
{
    /**
     * wraps T_DemandToProvider
     */
    public class DemandToProviderTable : IDemandToProviderTable
    {
        private List<T_DemandToProvider> _demandToProvider = new List<T_DemandToProvider>();

        public DemandToProviderTable(List<T_DemandToProvider> demandToProvider)
        {
            _demandToProvider = demandToProvider;
        }

        public DemandToProviderTable()
        {
        }

        public DemandToProviderTable(IDemandToProviders demandToProviders)
        {
            _demandToProvider.AddRange(demandToProviders.ToDemandToT_DemandToProvider());
        }

        public List<T_DemandToProvider> GetAll()
        {
            return _demandToProvider;
        }

        public void AddAll(IDemandToProviders demandToProviders)
        {
            _demandToProvider.AddRange(demandToProviders.ToDemandToT_DemandToProvider());
        }
    }
}