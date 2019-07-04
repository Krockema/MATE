using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.DemandDomain;
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

        void AddAll(Providers providers);
        
        List<Provider> GetAll();
        
        List<T> GetAllAs<T>();

        List<T_Provider> GetAllAsT_Provider();

        bool ProvideMoreThanOrEqualTo(Quantity quantity);
        
        int Size();
        
        bool Any();
        
        void Clear();

        bool AnyDependingDemands();

        Demands GetAllDependingDemands();
    }
}