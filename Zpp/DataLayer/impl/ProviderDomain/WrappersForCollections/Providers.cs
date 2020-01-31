using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections
{
    /**
     * wraps the collection with all providers
     */
    public class Providers : CollectionWrapperWithStackSet<Provider>
    {
        public Providers()
        {
        }

        public List<T> GetAllAs<T>()
        {
            List<T> providers = new List<T>();
            foreach (var provider in StackSet)
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
            Quantity providedQuantity = new Quantity(Quantity.Zero());

            foreach (var provider in StackSet)
            {
                if (articleId.Equals(provider.GetArticleId()))
                {
                    providedQuantity.IncrementBy(provider.GetQuantity());
                }
            }

            return providedQuantity;
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
                return Quantity.Zero();
            }

            return missingQuantity;
        }

        public Demands CalculateUnsatisfiedDemands(Demands demands)
        {
            Demands unSatisfiedDemands = new Demands();
            Dictionary<Provider, Quantity> reservableQuantityToProvider =
                new Dictionary<Provider, Quantity>();
            foreach (var provider in StackSet)
            {
                reservableQuantityToProvider.Add(provider, provider.GetQuantity());
            }

            foreach (var demand in demands.GetAll())
            {
                Quantity neededQuantity = demand.GetQuantity();
                foreach (var provider in StackSet)
                {
                    Quantity reservableQuantity = reservableQuantityToProvider[provider];
                    if (provider.GetArticleId().Equals(demand.GetArticleId()) &&
                        reservableQuantity.IsGreaterThan(Quantity.Zero()))
                    {
                        reservableQuantityToProvider[provider] = reservableQuantity
                            .Minus(neededQuantity);
                        neededQuantity = neededQuantity.Minus(reservableQuantity);

                        // neededQuantity < 0
                        if (neededQuantity.IsSmallerThan(Quantity.Zero()))
                        {
                            break;
                        }
                        // neededQuantity > 0: continue to provide it
                    }
                }

                if (neededQuantity.IsGreaterThan(Quantity.Zero()))
                {
                    unSatisfiedDemands.Add(demand);
                }
            }
            
            return unSatisfiedDemands;
        }

        public Provider GetProviderById(Id id)
        {
            // performance: cache this in a dictionary
            foreach (var provider in StackSet)
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
            foreach (var provider in StackSet)
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

        public INodes ToNodes()
        {
            INodes nodes = new Nodes();
            foreach (var item in StackSet)
            {
                nodes.Add(new Node(item));
            }

            return nodes;
        }
    }
}