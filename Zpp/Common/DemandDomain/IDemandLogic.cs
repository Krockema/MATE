using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {

        IDemand ToIDemand();

        Quantity GetQuantity();

        M_Article GetArticle();
        
        Id GetArticleId();

        DueTime GetDueTime(IDbTransactionData dbTransactionData);

        Id GetId();

        DueTime GetStartTime(IDbTransactionData dbTransactionData);
    }
}