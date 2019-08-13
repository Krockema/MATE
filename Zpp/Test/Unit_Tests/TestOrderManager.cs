using Master40.DB.Data.WrappersForPrimitives;
using Xunit;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.StockDomain;

namespace Zpp.Test
{
    public class TestOrderManager : AbstractTest
    {
        /**
        * Verifies, that 
        * - stock is increased correctly by demandQuantity after satisfy COP: stockCurrentAfter ==
               stockCurrentBefore + customerOrderPart.GetQuantity()
        */
        [Fact]
        public void TestSatisfyStockIsIncreasedAfterSatisfyStockExchangeDemand()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            StockManager stockManagerBefore = new StockManager(dbMasterDataCache.M_StockGetAll(), dbMasterDataCache);
            StockManager stockManagerAfter = new StockManager(dbMasterDataCache.M_StockGetAll(), dbMasterDataCache);
            IProviderManager providerManager =
                new ProviderManager(dbTransactionData);
            Demand customerOrderPart =
                EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(dbMasterDataCache, 2);
            IProvidingManager orderManager = new OrderManager(dbMasterDataCache);

            Response response = orderManager.Satisfy(customerOrderPart, customerOrderPart.GetQuantity(),
                dbTransactionData);
            MrpRun.ProcessProvidingResponse(response, providerManager, stockManagerAfter, dbTransactionData, customerOrderPart);

            Quantity beforeQuantity = stockManagerBefore
                .GetStockById(customerOrderPart.GetArticleId()).GetQuantity();
            Quantity afterQuantity = stockManagerAfter
                .GetStockById(customerOrderPart.GetArticleId()).GetQuantity();
            Assert.True(afterQuantity.Equals(beforeQuantity.Plus(customerOrderPart.GetQuantity())),
                "Stock was not correctly increased.");
        }
    }
}