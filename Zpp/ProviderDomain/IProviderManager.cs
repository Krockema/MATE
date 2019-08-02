using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    public interface IProviderManager
    {
        /**
         * @returns: the quantity that could be NOT reserved
         */
        Quantity ReserveQuantityOfExistingProvider(Id demandId, M_Article demandedArticle, Quantity demandedQuantity);

        /**
         * @returns: the quantity that is still NOT satisfied
         */
        Quantity AddProvider(Id demandId, Quantity demandedQuantity, Provider provider);
        
        /**
        * @returns: the quantity that is still NOT satisfied
        */
        Quantity AddProvider(Demand demand, Provider provider);

        /**
         * sum(quantity) over given demandId
         * aka select count(Quantity) where DemandId=demandId
         */
        Quantity GetSatisfiedQuantityOfDemand(Id demandId);
        
        bool IsSatisfied(Demand demand);
        
        /**
         * sum(quantity) over given providerId
         * aka select count(Quantity) where ProviderId=providerId
         */
        Quantity GetReservedQuantityOfProvider(Id providerId);

        /**
         * aka depending demands of new added providers.
         * NOTE: you can only call it once, then it will be cleared internally.
         */
        Demands GetNextDemands();

        IDemandToProviderTable GetDemandToProviderTable();

        IProviderToDemandTable GetProviderToDemandTable();

        IProviders GetProviders();
    }
}