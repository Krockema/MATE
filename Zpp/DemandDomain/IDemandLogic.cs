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

        DueTime GetStartTime(IDbTransactionData dbTransactionData);
    }
}