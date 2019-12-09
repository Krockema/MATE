using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Zpp;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Util;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class EntityFactory
    {
        private static Random random = new Random();

        /*public static T_ProductionOrderBom CreateT_ProductionOrderBom()
        {
            T_ProductionOrderBom tProductionOrderBom = new T_ProductionOrderBom();
            tProductionOrderBom.Quantity = new Random().Next(1, 100);
            M_Article article = dbMasterDataCache.M_ArticleGetById(
                IdGenerator.GetRandomId(0.M_ArticleGetAll().Count - 1));
            tProductionOrderBom.ArticleChild = article;
            tProductionOrderBom.ProductionOrderParent = CreateT_ProductionOrder()
        }*/

        public static ProductionOrder CreateT_ProductionOrder(
            
            Demand demand, Quantity quantity)
        {
            if (quantity == null || quantity.GetValue() == 0)
            {
                throw new MrpRunException("Quantity is not set.");
            }
            T_ProductionOrder tProductionOrder = new T_ProductionOrder();
            // [ArticleId],[Quantity],[Name],[DueTime],[ProviderId]
            tProductionOrder.DueTime = demand.GetStartTimeBackward().GetValue();
            tProductionOrder.Article = demand.GetArticle();
            tProductionOrder.ArticleId = demand.GetArticle().Id;
            tProductionOrder.Name = $"ProductionOrder for Demand {demand.GetArticle()}";
            // connects this provider with table T_Provider
            tProductionOrder.Quantity = quantity.GetValue(); // TODO: PASCAL .GetValueOrDefault();

            return new ProductionOrder(tProductionOrder);
        }

        private static T_CustomerOrder CreateCustomerOrder(DueTime dueTime)
        {
            IDbMasterDataCache dbMasterDataCache =
                ZppConfiguration.CacheManager.GetMasterDataCache();
            var order = new T_CustomerOrder()
            {
                BusinessPartnerId = dbMasterDataCache.M_BusinessPartnerGetAll()[0].Id,
                DueTime = dueTime.GetValue(),//,
                CreationTime = 0,
                Name = $"RandomProductionOrder{random.Next()}",
            };
            return order;
        }

        public static CustomerOrderPart CreateCustomerOrderPartRandomArticleToBuy(
            int quantity, DueTime dueTime)
        {
            IDbMasterDataCache dbMasterDataCache =
                ZppConfiguration.CacheManager.GetMasterDataCache();
            List<M_Article> articlesToBuy = dbMasterDataCache.M_ArticleGetArticlesToBuy();

            int randomArticleIndex = random.Next(0, articlesToBuy.Count - 1);
            CustomerOrderPart customerOrderPart =
                CreateCustomerOrderPartWithGivenArticle(quantity,
                    articlesToBuy[randomArticleIndex], dueTime);

            return customerOrderPart;
        }

        public static CustomerOrderPart CreateCustomerOrderPartWithGivenArticle(
            int quantity, M_Article article, DueTime dueTime)
        {
            T_CustomerOrder tCustomerOrder = CreateCustomerOrder(dueTime);
            T_CustomerOrderPart tCustomerOrderPart = new T_CustomerOrderPart();
            tCustomerOrderPart.CustomerOrder = tCustomerOrder;
            tCustomerOrderPart.CustomerOrderId = tCustomerOrder.Id;
            tCustomerOrderPart.Article = article;
            tCustomerOrderPart.ArticleId = tCustomerOrderPart.Article.Id;
            tCustomerOrderPart.Quantity = quantity;
            tCustomerOrderPart.State = State.Created;

            return new CustomerOrderPart(tCustomerOrderPart);
        }
    }
}