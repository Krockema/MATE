using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp
{
    /// <summary>
    /// An trivial implementation of IDemandManager, which is for performance reasons not well suited
    /// </summary>
    public class DemandManagerSimple : IDemandManager
    {
        private readonly List<IDemand> _demands = new List<IDemand>();
        private readonly Dictionary<int, List<int>> _demandsHavingProviders = new Dictionary<int, List<int>>();
        private readonly IProviderManager _providerManager;
        
        public DemandManagerSimple(IProviderManager providerManager)
        {
            _providerManager = providerManager;
            
        }
        
        public IDemand GetDemandById(int id)
        {
            foreach (IDemand demand in _demands)
            {
                if (demand.Id.Equals(id))
                {
                    return demand;
                }
            }

            return null;
        }

        public void AddDemand(IDemand demand)
        {
            _demands.Add(demand);
        }

        public List<IDemand> GetDemands()
        {
            return _demands;
        }
        
        public void orderDemandsByUrgency()
        {
            _demands.Sort((x, y) => x.GetDueTime().CompareTo(y.GetDueTime()));
        }

        public List<IProvider> getProvidersOfDemand(int demandId)
        {
            if (!_demandsHavingProviders.ContainsKey(demandId))
            {
                return null;
            }

            return _providerManager.getProvidersById(_demandsHavingProviders[demandId]);
        }

        public void addProviderForDemand(int demandId, int providerId)
        {
            if (!_demandsHavingProviders.ContainsKey(demandId))
            {
            
                _demandsHavingProviders.Add(demandId, new List<int>());
            }
            _demandsHavingProviders[demandId].Add(providerId);
        }
    }
}