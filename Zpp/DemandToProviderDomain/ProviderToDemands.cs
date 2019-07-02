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
    
    public class ProviderToDemands : IProviderToDemands
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

        public Providers GetAllProviders()
        {
            return new Providers(_providerToDemands.Keys.ToList());
        }
    }
}