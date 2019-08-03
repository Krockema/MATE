using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Xunit;

namespace Zpp.Test
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