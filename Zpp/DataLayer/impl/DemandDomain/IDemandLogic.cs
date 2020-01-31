using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.DataLayer.impl.DemandDomain
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {

        IDemand ToIDemand();

        M_Article GetArticle();
        
        Id GetArticleId();
    }
}