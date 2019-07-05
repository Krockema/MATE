using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Master40.DB;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zpp.DemandToProviderDomain;
using Zpp.Utils;

namespace Zpp.Test
{
    public class IntegrationTest : AbstractTest
    {
        private const int ORDER_QUANTITY = 6;

        public IntegrationTest()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }

        [Fact(Skip="not implemented yet")]
        public void TestBackwardScheduling()
        {
            MrpRun.RunMrp(ProductionDomainContext);
            
            
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData = new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            List<M_ArticleBom> rootArticles = dbMasterDataCache.M_ArticleBomGetRootArticles();

            foreach (var rootArticle in rootArticles)
            {
                // TODO: traverse the demandToProviders, not the articleTree
                ArticleTree articleTree = new ArticleTree(rootArticle, dbTransactionData);


                // traverse tree and execute an action
                TreeTools<M_Article>.traverseDepthFirst(articleTree, article =>
                {
                    // TODO: check now the backward scheduling
                });
            }
            
        }

        [Fact]
        public void TestTransactionDataIsCompletelyPersisted()
        {
            // TODO
        }

        /**
         * Verifies, that
         * - the sum(stockExchanges.Quantity) plus initial stock level equals stock.current
         * for every stock
         * - sum(stockExchangeDemands=withdrawel) <= sum(stockExchangeProviders=insert) for every stock
         */
        [Fact]
        public void TestStockExchanges()
        {
            IPlan plan = MrpRun.RunMrp(ProductionDomainContext);

            // stock behaviour
            IDbMasterDataCache originalDbMasterData =
                new DbMasterDataCache(ProductionDomainContext);
            IDbMasterDataCache nonPersistedDbMasterData = plan.GetNotPersistedDbMasterDataCache();
            IDbTransactionData persistedTransactionData =
                new DbTransactionData(ProductionDomainContext, originalDbMasterData);

            List<int> stockIdsFromPersistedStockExchanges = persistedTransactionData
                .StockExchangeGetAll().GetAllAs<T_StockExchange>().Select(x => x.StockId).ToList();
            List<M_Stock> originalStocks = originalDbMasterData.M_StockGetAll();
            foreach (var originalStock in originalStocks)
            {
                if (!stockIdsFromPersistedStockExchanges.Contains(originalStock.Id))
                {
                    // ignore all stocks for which there are no stockExchanges
                    continue;
                }

                decimal actualStockLevel = nonPersistedDbMasterData
                    .M_StockGetById(originalStock.GetId()).Current;

                // calculate the expected stock level (for every stock) from original master state
                // over all created stockExchanges
                // --> must be equal to current stock
                decimal expectedStockLevel = originalStock.Current;
                List<T_StockExchange> persistedStockExchanges = persistedTransactionData
                    .StockExchangeGetAll().GetAllAs<T_StockExchange>()
                    .Where(x => x.StockId.Equals(originalStock.Id)).ToList();
                decimal sumWithDrawel = 0;
                decimal sumInsert = 0;
                foreach (var persistedStockExchange in persistedStockExchanges)
                {
                    if (persistedStockExchange.ExchangeType.Equals(ExchangeType.Insert))
                    {
                        expectedStockLevel += persistedStockExchange.Quantity;
                        sumInsert += persistedStockExchange.Quantity;
                    }
                    else if (persistedStockExchange.ExchangeType.Equals(ExchangeType.Withdrawal))
                    {
                        expectedStockLevel -= persistedStockExchange.Quantity;
                        sumWithDrawel += persistedStockExchange.Quantity;
                    }
                }

                Assert.True(actualStockLevel.Equals(expectedStockLevel),
                    $"Stock level is not correct for stock {originalStock.Id}: " +
                    $"Expected: {expectedStockLevel}, Actual: {actualStockLevel}");

                Assert.True(sumWithDrawel <= sumInsert, "sumWithDrawel should be smaller than or equal to sumInsert");
            }
        }

        [Fact]
        public void TestMrpRun()
        {
            List<int> countsMasterDataBefore = CountMasterData();

            Assert.True(ProductionDomainContext.CustomerOrders.Count() == ORDER_QUANTITY,
                "No customerOrders are initially available.");

            IPlan plan = MrpRun.RunMrp(ProductionDomainContext);

            IDemands actualDemands = plan.GetDemands();

            IDemandToProviders demandToProviders = plan.GetDemandToProviders()
                .ToDemandToProviders(plan.GetDbTransactionData());
            foreach (var demand in actualDemands.GetAll())
            {
                bool isSatisfied = demandToProviders.IsSatisfied(demand);
                Assert.True(isSatisfied,
                    $"The demand {demand} should be satisfied, but it is NOT.");
            }

            // check certain constraints are not violated

            // masterData entities in db must not change within an MrpRun
            List<int> countsMasterDataAfter = CountMasterData();
            Assert.True(countsMasterDataBefore.SequenceEqual(countsMasterDataAfter),
                $"MasterData has changed, which should not be modified by MrpRun: " +
                $"\nBefore: {String.Join(", ", countsMasterDataBefore)}" +
                $"\nAfter: {String.Join(", ", countsMasterDataAfter)}");
        }


        private List<int> CountMasterData()
        {
            List<int> counts = new List<int>();
            counts.Add(ProductionDomainContext.Articles.Count());
            counts.Add(ProductionDomainContext.ArticleBoms.Count());
            counts.Add(ProductionDomainContext.ArticleTypes.Count());
            counts.Add(ProductionDomainContext.ArticleToBusinessPartners.Count());
            counts.Add(ProductionDomainContext.BusinessPartners.Count());
            counts.Add(ProductionDomainContext.Machines.Count());
            counts.Add(ProductionDomainContext.MachineGroups.Count());
            counts.Add(ProductionDomainContext.MachineTools.Count());
            counts.Add(ProductionDomainContext.Stocks.Count());
            counts.Add(ProductionDomainContext.Units.Count());
            counts.Add(ProductionDomainContext.Operations.Count());
            counts.Add(ProductionDomainContext.CustomerOrders.Count());
            counts.Add(ProductionDomainContext.CustomerOrderParts.Count());
            return counts;
        }
    }
}