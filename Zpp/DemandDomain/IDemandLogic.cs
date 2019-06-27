using System;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.DemandDomain
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {
        Provider CreateProvider(IDbTransactionData dbTransactionData);

        IDemand ToIDemand();

        bool HasProvider(Providers providers);

        Quantity GetQuantity();

        M_Article GetArticle();

        DueTime GetDueTime();

        Id GetId();
    }
}