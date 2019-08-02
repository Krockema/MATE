using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.Utils;
using Zpp.DemandDomain;

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

        public void AddAll(IProviders providers)
        {
            _providers.AddRange(providers.GetAll());
        }

        public List<Provider> GetAll()
        {
            return _providers;
        }

        public List<T> GetAllAs<T>()
        {
            List<T> providers = new List<T>();
            foreach (var provider in _providers)
            {
                providers.Add((T) provider.ToIProvider());
            }

            return providers;
        }

        public bool ProvideMoreThanOrEqualTo(Id articleId, Quantity demandedQuantity)
        {
            return GetProvidedQuantity(articleId).IsGreaterThanOrEqualTo(demandedQuantity);
        }

        public Quantity GetProvidedQuantity(Id articleId)
        {
            Quantity providedQuantity = new Quantity();

            foreach (var provider in _providers)
            {
                if (articleId.Equals(provider.GetArticleId()))
                {
                    providedQuantity.IncrementBy(provider.GetQuantity());
                }
            }

            return providedQuantity;
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

        public bool AnyDependingDemands()
        {
            foreach (var provider in _providers)
            {
                if (provider.AnyDependingDemands())
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSatisfied(Quantity demandedQuantity, Id articleId)
        {
            bool isSatisfied = ProvideMoreThanOrEqualTo(articleId, demandedQuantity);
            return isSatisfied;
        }

        public Quantity GetMissingQuantity(Quantity demandedQuantity, Id articleId)
        {
            Quantity missingQuantity = demandedQuantity.Minus(GetProvidedQuantity(articleId));
            if (missingQuantity.IsNegative())
            {
                return Quantity.Null();
            }

            return missingQuantity;
        }

        public IDemands CalculateUnsatisfiedDemands(IDemands demands)
        {
            List<Demand> unSatisfiedDemands = new List<Demand>();
            Dictionary<Provider, Quantity> reservableQuantityToProvider =
                new Dictionary<Provider, Quantity>();
            foreach (var provider in _providers)
            {
                reservableQuantityToProvider.Add(provider, provider.GetQuantity());
            }

            foreach (var demand in demands.GetAll())
            {
                Quantity neededQuantity = demand.GetQuantity();
                foreach (var provider in _providers)
                {
                    Quantity reservableQuantity = reservableQuantityToProvider[provider];
                    if (provider.GetArticleId().Equals(demand.GetArticleId()) &&
                        reservableQuantity.IsGreaterThan(Quantity.Null()))
                    {
                        reservableQuantityToProvider[provider] = reservableQuantity
                            .Minus(neededQuantity);
                        neededQuantity = neededQuantity.Minus(reservableQuantity);

                        // neededQuantity < 0
                        if (neededQuantity.IsSmallerThan(Quantity.Null()))
                        {
                            break;
                        }
                        // neededQuantity > 0: continue to provide it
                    }
                }

                if (neededQuantity.IsGreaterThan(Quantity.Null()))
                {
                    unSatisfiedDemands.Add(demand);
                }
            }
            
            return new Demands(unSatisfiedDemands);
        }

        public Provider GetProviderById(Id id)
        {
            // performance: cache this in a dictionary
            foreach (var provider in _providers)
            {
                if (provider.GetId().Equals(id))
                {
                    return provider;
                }
            }

            return null;
        }

        public List<Provider> GetAllByArticleId(Id id)
        {
            List<Provider> providers = new List<Provider>();
            // performance: cache this in a dictionary
            foreach (var provider in _providers)
            {
                if (provider.GetArticleId().Equals(id))
                {
                    providers.Add(provider);
                }
            }

            if (providers.Any() == false)
            {
                return null;
            }

            return providers;
        }
    }
}