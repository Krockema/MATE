using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.WrappersForCollections;

namespace Zpp.Common.ProviderDomain.WrappersForCollections
{
    /**
     * wraps the collection with all providers
     */
    public interface IProviders: ICollectionWrapper<Provider>
    {
        // TODO: Use this interface instead of the implementor Providers directly

        List<T> GetAllAs<T>();

        bool ProvideMoreThanOrEqualTo(Id articleId, Quantity demandedQuantity);

        void Clear();

        bool AnyDependingDemands();

        bool IsSatisfied(Quantity demandedQuantity, Id articleId);
        
        IDemands CalculateUnsatisfiedDemands(IDemands demands);

        Quantity GetMissingQuantity(Quantity demandedQuantity, Id articleId);

        // TODO: performance: use it less and cache it
        Quantity GetProvidedQuantity(Id articleId);

        Provider GetProviderById(Id id);

        List<Provider> GetAllByArticleId(Id id);
    }
}