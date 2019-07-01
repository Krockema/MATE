using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.Utils;
using Zpp.DemandDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * wraps the collection with all providers
     */
    public class Providers : IProviders
    {
        private readonly List<Provider> _providers = new List<Provider>();

        public Providers(List<Provider> providers)
        {
            if (providers == null)
            {
                throw new MrpRunException("Given list should not be null.");
            }
            _providers = providers;
        }

        public Providers(Provider[] providers)
        {
            if (providers == null)
            {
                throw new MrpRunException("Given list should not be null.");
            }
            List<Provider> providerList = new List<Provider>();
            foreach (var provider in providers)
            {
                providerList.Add(provider);
            }
            _providers = providerList;
        }

        public Providers(Provider provider1, Provider provider2)
        {
            _providers.Add(provider1);
            _providers.Add(provider2);
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

        public bool ProvideMoreThanOrEqualTo(Quantity quantity)
        {
            Quantity providedQuantity = new Quantity();
            
            foreach (var provider in _providers)
            {
                providedQuantity.IncrementBy(provider.GetQuantity());
            }

            return providedQuantity.IsGreaterThanOrEqualTo(quantity);
        }

        public int Size()
        {
            return _providers.Count;
        }

        public bool Any()
        {
            return _providers.Any();
        }

        public void Clear()
        {
            _providers.Clear();
        }

        public List<T_Provider> GetAllAsT_Provider()
        {
            return _providers.Select(x => x.ToT_Provider()).ToList();
        }
    }
}