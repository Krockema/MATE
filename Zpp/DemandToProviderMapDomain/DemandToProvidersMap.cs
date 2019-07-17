using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp.DemandToProviderDomain
{
    
    public class DemandToProvidersMap : IDemandToProvidersMap
    {
        private readonly Dictionary<Demand, IProviders> _demandToProviders = new Dictionary<Demand, IProviders>();
        private readonly IProviderToDemandsMap _providerToDemandsMap = new ProviderToDemandsMap();
        private const int MAX_PROVIDERS_PER_DEMAND = 2;

        public bool IsSatisfied(Demand demand)
        {
            bool isSatisfied = false;
            if (_demandToProviders.ContainsKey(demand))
            {
                IProviders providers = _demandToProviders[demand];
                isSatisfied = providers.IsSatisfied(demand);
                return isSatisfied;
            }

            return isSatisfied;
        }

        public void AddProviderForDemand(Demand demand, Provider provider)
        {
            if (!_demandToProviders.ContainsKey(demand))
            {
                _demandToProviders.Add(demand, new Providers());
            }
            if (_demandToProviders[demand].Size() > MAX_PROVIDERS_PER_DEMAND)
            {
                throw new MrpRunException($"One demand must not have more than {MAX_PROVIDERS_PER_DEMAND} providers.");
            }
            _demandToProviders[demand].Add(provider);
            _providerToDemandsMap.AddDemandForProvider(provider, demand);
            }

        public Provider FindNonExhaustedProvider(M_Article article)
        {
            return _providerToDemandsMap.FindNonExhaustedProvider(article);
        }

        public void AddProvidersForDemand(Demand demand, IProviders providers)
        {
            foreach (var provider in providers.GetAll())
            {
                AddProviderForDemand(demand, provider);
            }
        }

        public List<T_DemandToProvider> ToT_DemandToProvider()
        {
            List<T_DemandToProvider> demandToProvider = new List<T_DemandToProvider>();
            
            foreach (var demand in _demandToProviders.Keys)
            {
                foreach (var provider in _demandToProviders[demand].GetAll())
                {
                    T_DemandToProvider tDemandToProvider = new T_DemandToProvider();
                    tDemandToProvider.Demand = demand.ToT_Demand();
                    tDemandToProvider.DemandId = tDemandToProvider.Demand.Id;
                    tDemandToProvider.Provider = provider.ToT_Provider();
                    tDemandToProvider.ProviderId = tDemandToProvider.Provider.Id;
                    demandToProvider.Add(tDemandToProvider);
                }
            }
            
            return demandToProvider;
        }

        public List<T_Demand> ToT_Demands()
        {
            return new Demands(_demandToProviders.Keys.ToList()).GetAllAsT_Demand();
        }

        public List<T_Provider> ToT_Providers()
        {
            return _providerToDemandsMap.GetAllProviders().GetAllAsT_Provider();
        }

        public Demands GetDemands()
        {
            return new Demands(_demandToProviders.Keys.ToList());
        }
    }
}