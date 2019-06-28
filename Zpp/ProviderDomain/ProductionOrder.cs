using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.DemandDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.LotSize;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider, IProviderLogic
    {
        public ProductionOrder(IProvider provider, Demands demands, IDbMasterDataCache dbMasterDataCache) : base(provider, demands, dbMasterDataCache)
        {
        }

        public static ProductionOrder CreateProductionOrder(Demand demand,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache, ILotSize lotSize)
        {
            T_ProductionOrder productionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = demand.GetDueTime().GetValue();
            productionOrder.Article = demand.GetArticle();
            productionOrder.ArticleId = demand.GetArticle().Id;
            productionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = lotSize.GetCalculatedQuantity().GetValue();

            Demands newDemands = CreateProductionOrderBoms(demand,
                dbTransactionData, dbMasterDataCache, productionOrder, lotSize);

            return new ProductionOrder(productionOrder, newDemands, dbMasterDataCache);
        }

        private static Demands CreateProductionOrderBoms(Demand demand,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache, IProvider parentProductionOrder, ILotSize lotSize)
        {
            M_Article readArticle = dbTransactionData.M_ArticleGetById(demand.GetArticle().GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> productionOrderBoms = new List<Demand>();
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    ProductionOrderBom productionOrderBom = ProductionOrderBom.CreateProductionOrderBom(articleBom,
                        parentProductionOrder, dbMasterDataCache, lotSize);
                    productionOrderBoms.Add(productionOrderBom);
                }

                return new ProductionOrderBoms(productionOrderBoms);
            }

            return null;
        }

        public override IProvider ToIProvider()
        {
            return (T_ProductionOrder) _provider;
        }

        public override Id GetArticleId()
        {
            Id articleId = new Id(((T_ProductionOrder) _provider).ArticleId);
            return articleId;
        }
    }
}