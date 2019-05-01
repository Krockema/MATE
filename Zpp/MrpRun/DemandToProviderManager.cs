using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp
{
    public class DemandToProviderManager
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private readonly ProductionDomainContext _productionDomainContext;

        private readonly List<IDemand> _demands;
        private readonly List<IProvider> _providers;
        private readonly Dictionary<int, Lis> _demandsHavingProviders;
        // private readonly Dictionary<int, IProvider> providersAsDictionary;

        List<T_DemandToProvider> demandToProviders = new List<T_DemandToProvider>();

        Dictionary<int, T_DemandToProvider> providerToDemandToProvider =
            new Dictionary<int, T_DemandToProvider>();
        // demandToProvider: use 2 dicts for demandToProvider to get O(1) in both ways
        // Dictionary<int, int> demandToProvider = new Dictionary<int, int>();
        // Dictionary<int, int> providerToDemand = new Dictionary<int, int>();

        // getter/setter
        public List<IDemand> Demands => _demands;
        public List<IProvider> Providers => _providers;

        public DemandToProviderManager(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;

            _demands = ToIDemands(productionDomainContext.Demands.ToList());
            _providers = ToIProviders(productionDomainContext.Providers.ToList());
            demandsAsDictionary = ToIDemandsAsDictionary(_demands);
            providersAsDictionary = ToIProvidersAsDictionary(_providers);
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
            return t_providers.Select(x => x.ToIProvider(_productionDomainContext, x)).ToList();
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

        public void createProductionOrder()
        {
            LOGGER.Debug("ProductionOrder created.");
        }


        public void createStockExchange()
        {
            LOGGER.Debug("StockExchange created.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="demand"></param>
        /// <returns>the amount of not satisfied demand</returns>
        public int hasProvider(IDemand demand)
        {
            
        }
    }
}