using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp
{
    // <summary>
    /// An trivial implementation of IProviderManager, which is not well suited for performance reasons
    /// </summary>
    public class ProviderManagerSimple : IProviderManager
    {
        private readonly List<IProvider> _providers = new List<IProvider>();
        private readonly IDbCache _dbCache;

        public ProviderManagerSimple(IDbCache dbCache)
        {
            _dbCache = dbCache;
            _providers = ToIProviders(_dbCache);
        }

        public IProvider GetProviderById(int id)
        {
            foreach (IProvider provider in _providers)
            {
                if (provider.Id.Equals(id))
                {
                    return provider;
                }
            }

            return null;
        }

        public void AddProvider(IProvider provider)
        {
            _providers.Add(provider);
        }

        public List<IProvider> GetProviders()
        {
            return _providers;
        }

        public List<IProvider> GetProvidersById(List<int> ids)
        {
            return _providers.Where(x => ids.Contains(x.Id)).ToList();
        }

        private List<IProvider> ToIProviders(IDbCache dbCache)
        {
            if (dbCache.T_ProvidersGetAll() == null)
            {
                return new List<IProvider>();
            }
            return dbCache.T_ProvidersGetAll().Select(x => x.ToIProvider(x,
                dbCache.T_PurchaseOrderPartGetAll(), dbCache.T_ProductionOrderGetAll(),
                dbCache.T_StockExchangeGetAll())).ToList();
        }

        public void PersistProviders()
        {
            _dbCache.ProvidersAddAll(_providers);
            _providers.Clear();
        }
    }
}