using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Xunit;
using Zpp.DbCache;
using Zpp.Mrp.StockManagement;
using Zpp.WrappersForPrimitives;

namespace Master40.XUnitTest.Zpp.Unit_Tests.Provider
{
    public class TestStockExchangeProvider : AbstractTest
    {
        private Random random = new Random();

        public TestStockExchangeProvider()
        {
        }

        /**
         * Verifies, that created StockExchangeProvider has correct 
         * - Quantity (must equal quantity of demand),
         * - DueTime (must equal dueTime of demand),
         * - Article (must equal article of demand)
         * - exact one dependingDemands with quantity == (current-demanded) * (-1) if stock.min == 0 else quantity == stock.min(max would
         * lead to overfilled stock because most time it gets round up due to packsize)
         * - TODO: sync this with the test impl, since this description is always a step behind
         */
        [Fact]
        public void TestCreateStockExchangeProvider()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // CustomerOrderPart
            global::Zpp.Common.DemandDomain.Demand randomCustomerOrderPart =
                EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(dbMasterDataCache,
                    new Random().Next(3, 99), new DueTime(50));
            global::Zpp.Common.DemandDomain.Demand[] demands = new[]
            {
                randomCustomerOrderPart,
                EntityFactory.CreateCustomerOrderPartWithGivenArticle(dbMasterDataCache,
                    new Random().Next(1001, 1999), dbMasterDataCache.M_ArticleGetByName("Stahlrohr"), new DueTime(100)),
            };
            foreach (var demand in demands)
            {
                M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(demand.GetArticleId());

                StockManager stockManager = new StockManager(dbMasterDataCache.M_StockGetAll(), dbMasterDataCache);

                global::Zpp.Common.ProviderDomain.Provider providerStockExchange = stockManager.CreateStockExchangeProvider(
                    demand.GetArticle(), demand.GetDueTime(dbTransactionData), demand.GetQuantity(),
                    dbMasterDataCache, dbTransactionData);
                Assert.True((bool) providerStockExchange.GetQuantity().Equals(demand.GetQuantity()),
                    "Quantity is not correct.");
                Assert.True((bool) providerStockExchange.GetArticle().Equals(demand.GetArticle()),
                    "Article is not correct.");
                Assert.True(
                    (bool) providerStockExchange.GetDueTime(dbTransactionData)
                        .Equals(demand.GetDueTime(dbTransactionData)), "DueTime is not correct.");
            }
        }

        [Fact]
        public void TestNoDependingDemandsIfStockHasEnough()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // CustomerOrderPart
            global::Zpp.Common.DemandDomain.Demand demand =
                EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(dbMasterDataCache,
                    new Random().Next(1, 9), new DueTime(50));

            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(demand.GetArticleId());
            // increase stock
            stock.Current = 10;
            StockManager stockManager = new StockManager(dbMasterDataCache.M_StockGetAll(), dbMasterDataCache);
            global::Zpp.Common.ProviderDomain.Provider providerStockExchange = stockManager.CreateStockExchangeProvider(
                demand.GetArticle(), demand.GetDueTime(dbTransactionData), demand.GetQuantity(),
                dbMasterDataCache, dbTransactionData);

            
            Assert.True(providerStockExchange.AnyDependingDemands() == false, "Provider should have no depending demands.");
        }
    }
}