using Xunit;
using Zpp.DbCache;

namespace Zpp.Test.Unit_Tests.Demand
{
    public class TestStockExchangeDemand : AbstractTest
    {


        public TestStockExchangeDemand()
        {
            
        }
        
        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestCreateStockExchangeStockDemand()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            
            // TODO
            Assert.True(false);
        } 
    }
}