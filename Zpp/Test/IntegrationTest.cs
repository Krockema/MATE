using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zpp.DemandToProviderDomain;
using Zpp.Utils;

namespace Zpp.Test
{
    public class IntegrationTest : AbstractTest
    {
        private const int ORDER_QUANTITY = 1;
        private const int MAX_TIME_FOR_MRP_RUN = 60;

        public IntegrationTest()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }

        [Fact(Skip = "not implemented yet")]
        public void TestBackwardScheduling()
        {
            MrpRun.RunMrp(ProductionDomainContext);

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            List<M_ArticleBom> rootArticles = dbMasterDataCache.M_ArticleBomGetRootArticles();

            foreach (var rootArticle in rootArticles)
            {
                // TODO: traverse the demandToProviders, not the articleTree
                // --> therefore fill the new T_ProviderToDemand table
                ArticleTree articleTree = new ArticleTree(rootArticle, dbTransactionData);


                // traverse tree and execute an action
                TreeTools<M_Article>.traverseDepthFirst(articleTree, article =>
                {
                    // TODO: check now the backward scheduling
                });
            }
        }

        /**
         * Verifies, that the orderNet
         * - can be build up from DemandToProvider+ProviderToDemand table
         * - is a top down graph
         */
        [Fact]
        public void TestOrderNet()
        {
            MrpRun.RunMrp(ProductionDomainContext);

            // build it up

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            OrderGraph orderGraph = new OrderGraph(dbTransactionData);

            Assert.True(orderGraph.GetAdjacencyList().Values.Count > 0,
                "There are no child in the orderGraph.");

            int sumDemandToProviderAndProviderToDemand =
                dbTransactionData.DemandToProviderGetAll().Count() +
                dbTransactionData.ProviderToDemandGetAll().Count();

            // TODO: graphiz separate between label and id
            Assert.True(sumDemandToProviderAndProviderToDemand == orderGraph.Count(),
                $"Should be equal size: sumDemandToProviderAndProviderToDemand " +
                $"{sumDemandToProviderAndProviderToDemand} and  sumValuesOfOrderGraph {orderGraph.Count()}");

            // TODO: Lotsize must be higher
        }

        [Fact]
        public void TestTransactionDataIsCompletelyPersisted()
        {
            // TODO
        }

        [Fact]
        public void TestMaxTimeForMrpRunIsNotExceeded()
        {
            DateTime startTime = DateTime.UtcNow;

            MrpRun.RunMrp(ProductionDomainContext);

            DateTime endTime = DateTime.UtcNow;
            Assert.True((endTime - startTime).TotalMilliseconds / 1000 < MAX_TIME_FOR_MRP_RUN,
                $"MrpRun for example use case ({ORDER_QUANTITY} customerOrder) " +
                $"takes longer than {MAX_TIME_FOR_MRP_RUN} seconds");
        }

        /**
         * Verifies, that
         * - the sum(stockExchanges.Quantity) plus initial stock <= stock.Max && >= stock.Min
         * - 
         * - sum(stockExchangeDemands=withdrawal) <= sum(stockExchangeProviders=insert)
         * for every stock
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

                // =stock.Current
                decimal actualStockLevel = nonPersistedDbMasterData
                    .M_StockGetById(originalStock.GetId()).Current;

                // ExpectedStockLevel (=initial+sum(insert)-sum(withdrawal))
                decimal expectedStockLevel = originalStock.Current;
                Assert.True(expectedStockLevel >= 0,
                    $"Initial expectedStockLevel" +
                    $"({expectedStockLevel}) must be greaterThan/EqualTo 0.");

                List<T_StockExchange> persistedStockExchanges = persistedTransactionData
                    .StockExchangeGetAll().GetAllAs<T_StockExchange>()
                    .Where(x => x.StockId.Equals(originalStock.Id)).ToList();
                decimal sumWithDrawal = 0;
                decimal sumInsert = 0;

                // calculate the expected stock level (for every stock) from original master state
                // over all created stockExchanges
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
                        sumWithDrawal += persistedStockExchange.Quantity;
                    }
                }

                Assert.True(expectedStockLevel >= 0,
                    $"ExpectedStockLevel (=initial+sum(insert)-sum(withdrawal)) " +
                    $"({expectedStockLevel}) must be greaterThan/EqualTo 0.");
                Assert.True(actualStockLevel >= 0,
                    $"ActualStockLevel (=stock.Current) ({actualStockLevel}) " +
                    $"must be greaterThan/EqualTo 0.");

                decimal maxStock = originalStock.Max;
                if (maxStock < 1)
                {
                    maxStock = 5;
                }

                Assert.True(actualStockLevel < maxStock,
                    $"Stock level for stock {originalStock.Id} must be " +
                    $"smallerThan/equalTo MaxQuantity({maxStock}) " +
                    $"Expected: {maxStock}, Actual: {actualStockLevel}");

                decimal minStock = originalStock.Min;
                Assert.True(actualStockLevel >= minStock,
                    $"Stock level for stock {originalStock.Id} must be " +
                    $"smallerThan/equalTo MaxQuantity({minStock}) " +
                    $"Expected: {minStock}, Actual: {actualStockLevel}");

                Assert.True(sumWithDrawal <= sumInsert,
                    $"sumWithDrawel({sumWithDrawal}) should be smaller than or equal to sumInsert({sumInsert})");
            }
        }

        [Fact]
        public void TestAllDemandsAreSatisfiedByProvidersOfDemandToProviderTable()
        {
            MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDemandToProvidersMap demandToProvidersMap = dbTransactionData.DemandToProviderGetAll()
                .ToDemandToProvidersMap(dbTransactionData);
            IDemands allDbDemands = dbTransactionData.DemandsGetAll();
            foreach (var demand in allDbDemands.GetAll())
            {
                bool isSatisfied = demandToProvidersMap.IsSatisfied(demand);
                Assert.True(isSatisfied, $"Demand {demand} is not satisfied.");
            }
        }

        [Fact]
        public void TestAllDemandsAreInDemandToProviderTable()
        {
            MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDemandToProvidersMap demandToProvidersMap = dbTransactionData.DemandToProviderGetAll()
                .ToDemandToProvidersMap(dbTransactionData);
            IDemands allDbDemands = dbTransactionData.DemandsGetAll();
            Demands demandsInDemandToProviderTable = demandToProvidersMap.GetDemands();

            foreach (var demand in allDbDemands.GetAll())
            {
                bool isInDemandToProviderTable =
                    demandsInDemandToProviderTable.GetAll().Contains(demand);
                Assert.True(isInDemandToProviderTable,
                    $"Demand {demand} is NOT in demandToProviderTable.");
            }
        }

        /**
         * Tests, if the demands are theoretically satisfied by looking for providers in ProviderTable
         * --> success does not mean, that the demands from demandToProvider table are satisfied by providers from demandToProviderTable
         */
        [Fact]
        public void TestAllDemandsAreSatisfiedWithinProviderTable()
        {
            MrpRun.RunMrp(ProductionDomainContext);
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            IDemands demands = dbTransactionData.DemandsGetAll();
            IProviders providers = dbTransactionData.ProvidersGetAll();
            IDemands unsatisfiedDemands = providers.CalculateUnsatisfiedDemands(demands);
            foreach (var unsatisfiedDemand in unsatisfiedDemands.GetAll())
            {
                Assert.True(false,
                    $"The demand {unsatisfiedDemand} should be satisfied, but it is NOT.");
            }
        }

        [Fact]
        public void TestNoEntityWasLostDuringPersisting()
        {
            // TODO: check every created id is also in database
        }

        [Fact]
        public void TestMrpRun()
        {
            List<int> countsMasterDataBefore = CountMasterData();

            Assert.True(ProductionDomainContext.CustomerOrders.Count() == ORDER_QUANTITY,
                "No customerOrders are initially available.");

            IPlan plan = MrpRun.RunMrp(ProductionDomainContext);

            IDemands actualDemands = plan.GetDemands();

            IDemandToProvidersMap demandToProvidersMap = plan.GetDemandToProviders()
                .ToDemandToProvidersMap(plan.GetDbTransactionData());
            foreach (var demand in actualDemands.GetAll())
            {
                bool isSatisfied = demandToProvidersMap.IsSatisfied(demand);
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