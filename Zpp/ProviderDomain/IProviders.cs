using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    /**
     * wraps the collection with all providers
     */
    public interface IProviders
    {
        // TODO: Use this interface instead of the implementor Providers directly
        
        void Add(Provider provider);

        void AddAll(IProviders providers);
        
        List<Provider> GetAll();
        
        List<T> GetAllAs<T>();

        bool ProvideMoreThanOrEqualTo(Id articleId, Quantity demandedQuantity);
        
        int Size();
        
        bool Any();
        
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