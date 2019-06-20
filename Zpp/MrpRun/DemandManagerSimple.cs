using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp
{
    /// <summary>
    /// An simple implementation of IDemandManager, which is for performance reasons not well suited
    /// </summary>
    public class DemandManagerSimple : IDemandManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        private readonly List<IDemand> _demands;
        private bool IsDemandsListLocked = false;

        private readonly Dictionary<int, List<int>> _demandsHavingProviders =
            new Dictionary<int, List<int>>();

        private readonly IProviderManager _providerManager;
        private readonly IDbCache _dbCache; // is needed to persist demands at the end of MrpRun
        private readonly int _hierarchyNumber;

        /// <summary>
        /// Using this constructor, demandList initially has the already existing demands from database
        /// </summary>
        public DemandManagerSimple(IDbCache dbCache, IProviderManager providerManager, List<IDemand> demands)
        {
            _hierarchyNumber = 1;
            _providerManager = providerManager;
            _dbCache = dbCache;
            _demands = demands;
        }
        
        /// <summary>
        /// Using this constructor, demandList is initially empty
        /// </summary>
        public DemandManagerSimple(IDbCache dbCache, IProviderManager providerManager, int hierarchyNumber)
        {
            _providerManager = providerManager;
            _dbCache = dbCache;
            _demands = new List<IDemand>();
            _hierarchyNumber = hierarchyNumber;
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
            if (!IsDemandsListLocked)
            {
                _demands.Add(demand);    
            }
            else
            {
                LOGGER.Error($"Could not add given demand {demand.Id} to demandsList, because it's locked!");
            }
        }

        public List<IDemand> GetDemands()
        {
            return _demands;
        }

        public void OrderDemandsByUrgency()
        {
            _demands.Sort((x, y) => x.GetDueTime().CompareTo(y.GetDueTime()));
        }

        public List<IProvider> GetProvidersOfDemand(int demandId)
        {
            if (!_demandsHavingProviders.ContainsKey(demandId))
            {
                return null;
            }

            return _providerManager.GetProvidersById(_demandsHavingProviders[demandId]);
        }

        public void AddProviderForDemand(int demandId, int providerId)
        {
            if (!_demandsHavingProviders.ContainsKey(demandId))
            {
                _demandsHavingProviders.Add(demandId, new List<int>());
            }

            _demandsHavingProviders[demandId].Add(providerId);
        }

        public int GetHierarchyNumber()
        {
            return _hierarchyNumber;
        }

        public void LockDemandsList()
        {
            IsDemandsListLocked = true;
        }

        public void PersistDemands()
        {
            _dbCache.DemandsAddAll(_demands);
            _demands.Clear();
        }

        public void AddDemands(List<IDemand> demands)
        {
            foreach (IDemand demand in demands)
            {
                AddDemand(demand);
            }
        }
    }
}