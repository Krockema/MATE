using Xunit;
using Zpp.DbCache;

namespace Zpp.Test.Unit_Tests.Provider
{
    public class TestProvider : AbstractTest
    {


        public TestProvider()
        {

        }

        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestCreateNeededDemands()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            
            // TODO
            Assert.True(false);
        }

    }
}