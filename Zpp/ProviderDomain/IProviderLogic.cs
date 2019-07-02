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
        Demands GetDemands();

        IProvider ToIProvider();

        Quantity GetQuantity();

        bool AnyDemands();

        Id GetArticleId();
        
        M_Article GetArticle();

        bool ProvidesMoreThan(Quantity quantity);

        T_Provider ToT_Provider();

        Demands CreateNeededDemands(M_Article article, IDbTransactionData dbTransactionData,
            IDbMasterDataCache dbMasterDataCache, Provider parentProvider, Quantity quantity);

        DueTime GetDueTime();
    }
}