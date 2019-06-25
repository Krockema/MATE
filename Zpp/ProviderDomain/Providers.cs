using System.Collections.Generic;
using Master40.DB.Data.Context;

namespace Zpp.ProviderDomain
{
    /**
     * wraps the collection with all providers
     */
    public class Providers : IProviders
    {
        private List<Provider> _providers;

        public Providers(List<Provider> providers)
        {
            _providers = providers;
        }

        public Providers()
        {
        }

        public void Add(Provider provider)
        {
            _providers.Add(provider);
        }

        public void AddAll(Providers providers)
        {
            _providers.AddRange(providers.GetAll());
        }

        public List<Provider> GetAll()
        {
            return _providers;
        }
        
        public List<T> GetAllAs<T>()
        {
            List<T> productionOrderBoms = new List<T>();
            foreach (var demand in _providers)
            {
                productionOrderBoms.Add((T)demand.ToIProvider());
            }
            return productionOrderBoms;
        }
    }
}