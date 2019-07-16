using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp.DemandToProviderDomain
{
    /**
     * Maps one provider to demands. A provider can satisfy possibly n demands
     */
    public interface IProviderToDemandsMap
    {
        /// <summary>
        /// demand is added to the provider
        /// </summary>
        /// <param name="provider">that gets a new demand</param>
        /// <param name="demand">that is added to a provider</param>
        void AddDemandForProvider(Provider provider, Demand demand);
        
        void AddDemandsForProvider(Provider provider, IDemands demands);
        
        void AddAll(IProviderToDemandsMap providerToDemandsMap);

        Provider FindNonExhaustedProvider(M_Article article);

        IProviders GetAllProviders();
        
        IDemands GetAllDemands();
        
        IDemands GetAllDemandsOfProvider(Provider provider);
        
        /**
         * Converts to list of T_ProviderToDemand
         */
        List<T_ProviderToDemand> ToT_ProviderToDemand();
        
    }
}