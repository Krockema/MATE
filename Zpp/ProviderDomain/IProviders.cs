using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp;
using Zpp.DemandDomain;
using Zpp.DemandToProviderDomain;
using Zpp.WrappersForPrimitives;

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

        List<T_Provider> GetAllAsT_Provider();

        bool ProvideMoreThanOrEqualTo(Demand demand);
        
        int Size();
        
        bool Any();
        
        void Clear();

        bool AnyDependingDemands();

        IProviderToDemandsMap GetAllDependingDemandsAsMap();

        bool IsSatisfied(Demand demand);

        Quantity GetMissingQuantity(Demand demand);

        Quantity GetProvidedQuantity(Demand demand);
    }
}