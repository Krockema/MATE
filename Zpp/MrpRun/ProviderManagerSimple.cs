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

        public ProviderManagerSimple()
        {

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

        public List<IProvider> getProvidersById(List<int> ids)
        {
            return _providers.Where(x=>ids.Contains(x.Id)).ToList();
        }
    }
}