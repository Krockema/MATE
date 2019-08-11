using System;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.StockDomain;

namespace Zpp.DemandDomain
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {

        IDemand GetIDemand();

        Quantity GetQuantity();

        M_Article GetArticle();
        
        Id GetArticleId();

        DueTime GetDueTime(IDbTransactionData dbTransactionData);

        Id GetId();

        /**
         * For convenience
         */
        Quantity SatisfyByExistingNonExhaustedProvider(IProviderManager providerManager,
            Demand demand, Quantity remainingQuantity);

        Quantity SatisfyByStock(Quantity remainingQuantity, IDbTransactionData dbTransactionData,
            IProviderManager providerManager, Demand demand, StockManager stockManager);

        Quantity SatisfyByOrders(IDbTransactionData dbTransactionData, Quantity remainingQuantity,
            IProviderManager providerManager, Demand demand);

    }
}