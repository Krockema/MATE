using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_PurchaseOrderPart
     */
    public class PurchaseOrderPart : Provider, IProviderLogic
    {
        public PurchaseOrderPart(IProvider provider, Demands demands, IDbMasterDataCache dbMasterDataCache) : base(provider, demands, dbMasterDataCache)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_PurchaseOrderPart)_provider;
        }

        public override Id GetArticleId()
        {
            Id articleId = new Id(((T_PurchaseOrderPart)_provider).ArticleId);
            return articleId;
        }
    }
}