using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;
using ZppForPrimitives;

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

        public ProductionOrder(IDemand demand, IDbCache dbCache) : base(
            CreateProductionOrder(demand), CreateProductionOrderBoms(demand,
            dbCache))

        {
        }

        private static IProvider CreateProductionOrder(IDemand demand)
        {
            T_ProductionOrder productionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            productionOrder.DueTime = demand.GetDueTime();
            productionOrder.Article = demand.GetArticle();
            productionOrder.ArticleId = demand.GetArticle().Id;
            productionOrder.Name = $"ProductionOrder for Demand {demand.Id}";
            // connects this provider with table T_Provider
            productionOrder.Provider = new T_Provider();
            productionOrder.Quantity = demand.GetQuantity().GetValue();


            return productionOrder;
        }

        private static Demands CreateProductionOrderBoms(IDemand demand, IDbCache dbCache)
        {
            M_Article readArticle = dbCache.M_ArticleGetById(demand.GetArticle().Id);
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> productionOrderBoms = new List<Demand>();
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    ProductionOrderBom productionOrderBom = new ProductionOrderBom(articleBom,
                        CreateProductionOrder(demand));
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