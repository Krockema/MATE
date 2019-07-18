using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp.DemandToProviderDomain
{
    
    public class ProviderToDemandsMap : IProviderToDemandsMap
    {
        private readonly Dictionary<Provider, IDemands> _providerToDemands = new Dictionary<Provider, IDemands>();
        private readonly Dictionary<Id, Providers> _articleToProviders = new Dictionary<Id, Providers>();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void AddDemandForProvider(Provider provider, Demand demand)
        {
            // add provider
            if (!_providerToDemands.ContainsKey(provider))
            {
                _providerToDemands.Add(provider, new Demands());
                return;
            }
            _providerToDemands[provider].Add(demand);
            
            // add article
            Id articleId = demand.GetArticleId();
            if (!_articleToProviders.ContainsKey(articleId))
            {
                _articleToProviders.Add(articleId, new Providers());
            }
            _articleToProviders[articleId].Add(provider);

        }

        public Provider FindNonExhaustedProvider(M_Article article)
        {
            if (!_articleToProviders.ContainsKey(article.GetId()))
            {
                return null;
            }
                
            foreach (var provider in _articleToProviders[article.GetId()].GetAll())
            {
                Quantity quantityOfAllDemands = _providerToDemands[provider].GetQuantityOfAll();
                if (provider.GetQuantity().IsGreaterThanOrEqualTo(quantityOfAllDemands))
                {
                    return provider;
                }
            }

            return null;
        }

        public IProviders GetAllProviders()
        {
            return new Providers(_providerToDemands.Keys.ToList());
        }

        public IDemands GetAllDemands()
        {
            IDemands allDemands = new Demands();
            foreach (var oneDemands in _providerToDemands.Values.ToList())
            {
                allDemands.AddAll(oneDemands);
            }
            return allDemands;
        }

        public void AddDemandsForProvider(Provider provider, IDemands demands)
        {
            if (!_providerToDemands.ContainsKey(provider))
            {
                _providerToDemands.Add(provider, demands);
                return;
            }
            _providerToDemands[provider].AddAll(demands);
        }

        public IDemands GetAllDemandsOfProvider(Provider provider)
        {
            if (_providerToDemands.ContainsKey(provider))
            {
                return _providerToDemands[provider];
            }

            return null;
        }

        public void AddAll(IProviderToDemandsMap providerToDemandsMap)
        {
            foreach (var provider in providerToDemandsMap.GetAllProviders().GetAll())
            {
                IDemands demands = providerToDemandsMap.GetAllDemandsOfProvider(provider);
                if (demands != null && demands.Any())
                {
                    AddDemandsForProvider(provider, demands);    
                }
                
            }
        }

        public List<T_ProviderToDemand> ToT_ProviderToDemand()
        {
            List<T_ProviderToDemand> providerToDemand = new List<T_ProviderToDemand>();
            
            foreach (var provider in _providerToDemands.Keys)
            {
                foreach (var demand in _providerToDemands[provider].GetAll())
                {
                    T_ProviderToDemand tProviderToDemand = new T_ProviderToDemand();
                    tProviderToDemand.DemandId = demand.GetId().GetValue();
                    tProviderToDemand.ProviderId = provider.GetId().GetValue();
                    providerToDemand.Add(tProviderToDemand);
                }
            }
            
            return providerToDemand;
        }
    }
}