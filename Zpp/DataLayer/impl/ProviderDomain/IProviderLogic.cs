using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.DataLayer.impl.ProviderDomain
{
    /**
     * A wrapper for IProvider providing methods that every wrapped ProviderType implements
     */
    public interface IProviderLogic
    {

        IProvider ToIProvider();

        Id GetArticleId();
        
        M_Article GetArticle();

        bool ProvidesMoreThan(Quantity quantity);

        void SetProvided(DueTime atTime);
    }
}