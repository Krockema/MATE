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
        private readonly List<IProvider> DEMANDS = new List<IProvider>();

        public ProviderManagerSimple()
        {

        }

        public IProvider GetProviderById(int id)
        {
            foreach (IProvider demand in DEMANDS)
            {
                if (demand.Id.Equals(id))
                {
                    return demand;
                }
            }

            return null;
        }

        public void AddProvider(IProvider demand)
        {
            DEMANDS.Add(demand);
        }

        public List<IProvider> GetProviders()
        {
            return DEMANDS;
        }
        
        public List<IProvider> ToIProviders(List<T_Provider> t_providers)
        {
            return t_providers.Select(x => x.ToIProvider(_productionDomainContext, x)).ToList();
        }

    }
}