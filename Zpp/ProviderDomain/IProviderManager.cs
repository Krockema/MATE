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
         * @returns: the quantity that could be NOT reserved. Must read providers from db.
         */
        Quantity ReserveQuantityOfExistingProvider(Id demandId, M_Article demandedArticle, Quantity demandedQuantity);

        /**
         * @returns: (demandedQuantity - provider.getQuantity()), Quantity.Null if provider.getQuantity() is bigger than demandedQuantity
         */
        void AddProvider(Id demandId, Quantity demandedQuantity, Provider provider, Quantity reservedQuantity);
        
        /**
        * - adapts the stock: StockExchangeProvider decrement, else increment
        * @returns: the quantity that is still NOT satisfied
        */
        void AddProvider(Demand demand, Provider provider, Quantity reservedQuantity);

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