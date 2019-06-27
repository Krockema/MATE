using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.DemandDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_ProductionOrder
     */
    public class ProductionOrder : Provider, IProviderLogic
    {
        public ProductionOrder(IProvider provider, Demands demands) : base(provider, demands)
        {
        }

        public ProductionOrder(Demand demand, IDbTransactionData dbTransactionData,
            IDbCacheMasterData dbCacheMasterData) : base(CreateProductionOrder(demand),
            CreateProductionOrderBoms(demand, dbTransactionData, dbCacheMasterData))

        {
        }

        private static IProvider CreateProductionOrder(Demand demand)
        {
            T_ProductionOrder productionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = demand.GetDueTime().GetValue();
            productionOrder.Article = demand.GetArticle();
            productionOrder.ArticleId = demand.GetArticle().Id;
            productionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = demand.GetQuantity().GetValue();


            return productionOrder;
        }

        private static Demands CreateProductionOrderBoms(Demand demand,
            IDbTransactionData dbTransactionData, IDbCacheMasterData dbCacheMasterData)
        {
            M_Article readArticle = dbTransactionData.M_ArticleGetById(demand.GetArticle().GetId());
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> productionOrderBoms = new List<Demand>();
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    ProductionOrderBom productionOrderBom = new ProductionOrderBom(articleBom,
                        CreateProductionOrder(demand), dbCacheMasterData);
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
    }
}