using System;
using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.DemandDomain;

namespace Zpp.Test
{
    public class EntityFactory
    {
        private static Random random = new Random();
        
        /*public static T_ProductionOrderBom CreateT_ProductionOrderBom(IDbMasterDataCache dbMasterDataCache)
        {
            T_ProductionOrderBom tProductionOrderBom = new T_ProductionOrderBom();
            tProductionOrderBom.Quantity = new Random().Next(1, 100);
            M_Article article = dbMasterDataCache.M_ArticleGetById(
                IdGenerator.GetRandomId(0, dbMasterDataCache.M_ArticleGetAll().Count - 1));
            tProductionOrderBom.ArticleChild = article;
            tProductionOrderBom.ProductionOrderParent = CreateT_ProductionOrder(dbMasterDataCache, )
        }*/

        public static T_ProductionOrder CreateT_ProductionOrder(
            IDbMasterDataCache dbMasterDataCache, Demand demand, Quantity quantity)
        {
            T_ProductionOrder tProductionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            tProductionOrder.DueTime = demand.GetDueTime().GetValue();
            tProductionOrder.Article = demand.GetArticle();
            tProductionOrder.ArticleId = demand.GetArticle().Id;
            tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            // connects this provider with table T_Provider
            tProductionOrder.Quantity = quantity.GetValue();

            return tProductionOrder;
        }

        public static T_CustomerOrder CreateCustomerOrder(IDbMasterDataCache dbMasterDataCache)
        {
            var order = new T_CustomerOrder()
            {
                BusinessPartnerId = dbMasterDataCache.M_BusinessPartnerGetAll()[0].Id,
                DueTime = random.Next(5, 1000),
                CreationTime = 0,
                Name = $"RandomProductionOrder{random.Next()}",
                
            };
            return order;
        }

        public static CustomerOrderPart CreateCustomerOrderPart(IDbMasterDataCache dbMasterDataCache, int quantity)
        {
            T_CustomerOrder tCustomerOrder = CreateCustomerOrder(dbMasterDataCache);
            T_CustomerOrderPart tCustomerOrderPart = new T_CustomerOrderPart();
            tCustomerOrderPart.CustomerOrder = tCustomerOrder;
            tCustomerOrderPart.CustomerOrderId = tCustomerOrder.Id;
            int randomArticleIndex =
                random.Next(0, dbMasterDataCache.M_ArticleGetAll().Count - 1);
            tCustomerOrderPart.Article = dbMasterDataCache.M_ArticleGetAll()[randomArticleIndex];
            tCustomerOrderPart.ArticleId = tCustomerOrderPart.Article.Id;
            tCustomerOrderPart.Quantity = quantity;
            tCustomerOrderPart.State = State.Created;

            return new CustomerOrderPart(tCustomerOrderPart, dbMasterDataCache);
        }
    }
}