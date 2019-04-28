using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp
{
    public class DemandToProviderManager
    {
        private readonly ProductionDomainContext _productionDomainContext;

        public DemandToProviderManager(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;
        }

        public void orderDemandsByUrgency(List<IDemand> demands)
        {
            demands.Sort((x, y) => x.GetDueTime().CompareTo(y.GetDueTime()));
        }

        public List<IDemand> ToIDemands(List<T_Demand> t_demands)
        {
           
            return t_demands.Select(x => x.ToIDemand(_productionDomainContext, x)).ToList();
        }

        public List<IProvider> ToIProviders(List<T_Provider> t_providers)
        {
            return t_providers
                .Select(x => x.ToIProvider(_productionDomainContext, x))
                .ToList();
        }
        
        public Dictionary<int, IDemand> ToIDemandsAsDictionary(List<IDemand> demands)
        {
           
            Dictionary<int, IDemand> demandsAsDictionary = new Dictionary<int, IDemand>();
            foreach (IDemand demand in demands)
            {
                demandsAsDictionary.Add(demand.Id, demand);
            }

            return demandsAsDictionary;
        }

        public Dictionary<int, IProvider> ToIProvidersAsDictionary(List<IProvider> providers)
        {
            Dictionary<int, IProvider> providersAsDictionary = new Dictionary<int, IProvider>();
            foreach (IProvider provider in providers)
            {
                providersAsDictionary.Add(provider.Id, provider);
            }

            return providersAsDictionary;
        }
    }
}