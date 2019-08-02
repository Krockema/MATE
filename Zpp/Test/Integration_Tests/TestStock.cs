using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;

namespace Zpp.Test
{
    public class TestStock : AbstractTest
    {
        private const int ORDER_QUANTITY = 1;

        public TestStock()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }
    
        /**
         * Verifies, that
         * - the sum(stockExchanges.Quantity) plus initial stock <= stock.Max && >= stock.Min
         * - 
         * - sum(stockExchangeDemands=withdrawal) <= sum(stockExchangeProviders=insert)
         * for every stock
         * TODO: sync this describtion with the impl
         */
        [Fact]
        public void TestStockExchanges()
        {
            MrpRun.RunMrp(ProductionDomainContext);

            // stock behaviour
            IDbMasterDataCache originalDbMasterData =
                new DbMasterDataCache(ProductionDomainContext);
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

                // currentStockLevel will be calculated (=initial+sum(insert)-sum(withdrawal))
                decimal currentStockLevel = originalStock.Current;
                Assert.True(currentStockLevel >= 0,
                    $"Initial expectedStockLevel" +
                    $"({currentStockLevel}) must be greaterThan/EqualTo 0.");

                List<T_StockExchange> persistedStockExchanges = persistedTransactionData
                    .StockExchangeGetAll().GetAllAs<T_StockExchange>()
                    .Where(x => x.StockId.Equals(originalStock.Id)).ToList();
                decimal sumWithDrawal = 0;
                decimal sumInsert = 0;

                // calculate the expected stock level (for every stock) from original master state
                // over all created stockExchanges
                foreach (var persistedStockExchange in persistedStockExchanges)
                {
                    if (persistedStockExchange.ExchangeType.Equals(ExchangeType.Insert) ||
                        persistedStockExchange.StockExchangeType.Equals(StockExchangeType.Provider))
                    {
                        currentStockLevel += persistedStockExchange.Quantity;
                        sumInsert += persistedStockExchange.Quantity;
                    }
                    else
                    {
                        currentStockLevel -= persistedStockExchange.Quantity;
                        sumWithDrawal += persistedStockExchange.Quantity;
                    }
                }

                Assert.True(currentStockLevel >= 0,
                    $"ExpectedStockLevel (=initial+sum(insert)-sum(withdrawal)) " +
                    $"({currentStockLevel}) must be greaterThan/EqualTo 0.");

                decimal maxStock = originalStock.Max;
                Assert.True(maxStock > 0, $"stock.max for {originalStock.Article} must be greater than zero.");

                Assert.True(currentStockLevel < maxStock + new decimal(0.01),
                    $"Stock level for stock {originalStock.Id} must be " +
                    $"smallerThan/equalTo MaxQuantity({maxStock}) " +
                    $"Expected: {maxStock}, Actual: {currentStockLevel}");

                decimal minStock = originalStock.Min;
                Assert.True(currentStockLevel >= minStock,
                    $"Stock level for stock {originalStock.Id} must be " +
                    $"greaterThan/equalTo MinQuantity({minStock}) " +
                    $"Expected: {minStock}, Actual: {currentStockLevel}");

                Assert.True(sumWithDrawal <= sumInsert,
                    $"sumWithDrawel({sumWithDrawal}) should be smaller than or equal to sumInsert({sumInsert})");
            }
        }

        
    }
}