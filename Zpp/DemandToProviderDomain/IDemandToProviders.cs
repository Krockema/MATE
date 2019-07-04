using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    /**
     * Maps one demand to max. two providers (at least one of the providers must be exhausted,
     * the demand must be satisfied by both providers)
     *
     * NOTE: the "s" at the end, has nothing to do with T_DemandToProvider table & IDemandToProvider interface (Those are dbRelated)
     */
    public interface IDemandToProviders
    {
        bool IsSatisfied(Demand demand);

        /// <summary>
        /// given provider is added to given demand
        /// </summary>
        /// <param name="demand">that gets a new provider</param>
        /// <param name="provider">that is added to demand</param>
        void AddProviderForDemand(Demand demand, Provider provider);

        /**
         * Should  do "AddProviderForDemand" for every provider of given providers
         */
        void AddProvidersForDemand(Demand demand, IProviders providers);

        Provider FindNonExhaustedProvider(M_Article article);
        
        /**
         * Converts to list of T_DemandToProvider
         */
        List<T_DemandToProvider> ToDemandToT_DemandToProvider();

        List<T_Demand> ToT_Demands();
        
        List<T_Provider> ToT_Providers();
    }
}