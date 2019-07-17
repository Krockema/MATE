using System;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DemandToProviderDomain;

namespace Zpp.DemandDomain
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {
        

        IDemand ToIDemand();
        
        T_Demand ToT_Demand();
        
        T_Demand ToT_Demand(IDbTransactionData dbTransactionData);

        Quantity GetQuantity();

        M_Article GetArticle();
        
        Id GetArticleId();

        DueTime GetDueTime();

        Id GetId();

        Id GetT_DemandId();

        IProviders Satisfy(IDemandToProvidersMap demandToProvidersMap,
            IDbTransactionData dbTransactionData);

        /**
         * For convenience
         */
        Provider SatisfyByExistingNonExhaustedProvider(IDemandToProvidersMap demandToProvidersMap,
            M_Article article);

        IProviders SatisfyByStock(Quantity missingQuantity, IDbTransactionData dbTransactionData);

        IProviders SatisfyByOrders(IDbTransactionData dbTransactionData, Quantity quantity);

    }
}