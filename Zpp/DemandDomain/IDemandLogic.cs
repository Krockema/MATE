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
        Provider CreateProvider(IDbTransactionData dbTransactionData, Quantity quantity);

        IDemand ToIDemand();
        
        T_Demand ToT_Demand();

        Quantity GetQuantity();

        M_Article GetArticle();
        
        Id GetArticleId();

        DueTime GetDueTime();

        Id GetId();

        Providers Satisfy(IDemandToProviders demandToProviders, IDbTransactionData dbTransactionData,
            Demands nextDemands);
        
    }
}