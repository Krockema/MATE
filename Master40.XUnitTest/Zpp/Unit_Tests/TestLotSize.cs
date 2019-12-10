using Master40.DB.Data.WrappersForPrimitives;
using Xunit;
using Zpp.DbCache;
using Zpp.LotSize;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestLotSize : AbstractTest
    {
        [Fact]
        public void TestALotSize()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            LotSize lotSize = new LotSize(new Quantity(6),
                dbMasterDataCache.M_ArticleGetAll()[0].GetId(), dbMasterDataCache);
            foreach (var quantity in lotSize.GetLotSizes())
            {
                Assert.True((bool) (quantity.GetValue() == TestConfiguration.LotSize), $"Quantity ({quantity}) is not correct.");
            }
        }
    }
}