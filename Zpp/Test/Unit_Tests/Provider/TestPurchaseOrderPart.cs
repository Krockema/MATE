using Xunit;
using Zpp.DbCache;

namespace Zpp.Test.Unit_Tests.Provider
{
    public class TestPurchaseOrderPart : AbstractTest
    {


        public TestPurchaseOrderPart()
        {
            
        }
        
        /**
         * Verifies, that 
         * - 
         */
        [Fact(Skip = "Not implemented yet.")]
        public void TestPurchaseOrderPartCreatePurchaseOrderPart()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            
            // TODO
            Assert.True(false);
        }            

    }
}