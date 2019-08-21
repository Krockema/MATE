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
    public class Providers : CollectionWrapperWithList<Provider>, IProviders
    {
        public Providers(List<Provider> list) : base(list)
        {
        }

        public Providers()
        {
        }

        public List<T> GetAllAs<T>()
        {
            List<T> providers = new List<T>();
            foreach (var provider in List)
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
            Quantity providedQuantity = new Quantity(Quantity.Null());

            foreach (var provider in List)
            {
                if (articleId.Equals(provider.GetArticleId()))
                {
                    providedQuantity.IncrementBy(provider.GetQuantity());
                }
            }

            return providedQuantity;
        }

        public void Clear()
        {
            List.Clear();
        }

        public bool AnyDependingDemands()
        {
            foreach (var provider in List)
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
            foreach (var provider in List)
            {
                reservableQuantityToProvider.Add(provider, provider.GetQuantity());
            }

            foreach (var demand in demands.GetAll())
            {
                Quantity neededQuantity = demand.GetQuantity();
                foreach (var provider in List)
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
            foreach (var provider in List)
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
            foreach (var provider in List)
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