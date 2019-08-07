using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.StockDomain;
using Zpp.Test.WrappersForPrimitives;

namespace Zpp.Test
{
    public class TestDemand : AbstractTest
    {
        public TestDemand()
        {
        }

        /**
         * Verifies, that 
         * - stock is increased correctly by demandQuantity after satisfy COP: stockCurrentAfter ==
                stockCurrentBefore + customerOrderPart.GetQuantity()
         */
        [Fact]
        public void TestStockIsIncreasedAfterSatisfyStockExchangeDemand()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            StockManager stockManagerBefore = new StockManager(dbMasterDataCache.M_StockGetAll());
            StockManager stockManagerAfter = new StockManager(dbMasterDataCache.M_StockGetAll());
            IProviderManager providerManager = new ProviderManager(stockManagerAfter);
            Demand customerOrderPart =
                EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(dbMasterDataCache, 2);
            decimal stockCurrentBefore = dbMasterDataCache
                .M_StockGetByArticleId(customerOrderPart.GetArticleId()).Current;

            customerOrderPart.SatisfyStockExchangeDemand(providerManager, dbTransactionData);

            decimal stockCurrentAfter = dbMasterDataCache
                .M_StockGetByArticleId(customerOrderPart.GetArticleId()).Current;

            Assert.True(
                stockCurrentAfter ==
                stockCurrentBefore + customerOrderPart.GetQuantity().GetValue(),
                "Stock was not correctly increased.");
            Quantity beforeQuantity = stockManagerBefore
                .GetStockById(customerOrderPart.GetArticleId()).GetQuantity();
            Quantity afterQuantity = stockManagerAfter
                .GetStockById(customerOrderPart.GetArticleId()).GetQuantity();
            Assert.True(
                afterQuantity.Equals(beforeQuantity.Plus(customerOrderPart.GetQuantity())),
                "Stock was not correctly increased.");
        }

        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestSatisfyByExistingNonExhaustedProvider()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // TODO
            Assert.True(false);
        }

        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestSatisfyByStock()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // TODO
            Assert.True(false);
        }

        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestSatisfyByOrders()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            // TODO
            Assert.True(false);
        }
    }
}