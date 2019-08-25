using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.DemandDomain;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.WrappersForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * A wrapper for IProvider providing methods that every wrapped ProviderType implements
     */
    public interface IProviderLogic
    {
        Demands GetAllDependingDemands();

        IProvider ToIProvider();

        Quantity GetQuantity();

        bool AnyDependingDemands();

        Id GetArticleId();
        
        M_Article GetArticle();

        bool ProvidesMoreThan(Quantity quantity);

        void CreateDependingDemands(M_Article article, IDbTransactionData dbTransactionData,
            Provider parentProvider, Quantity quantity);

        DueTime GetDueTime(IDbTransactionData dbTransactionData);

        Id GetId();
        
        DueTime GetStartTime(IDbTransactionData dbTransactionData);

    }
}