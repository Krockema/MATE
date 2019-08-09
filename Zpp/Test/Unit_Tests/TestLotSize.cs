using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Xunit;

namespace Zpp.Test
{
    public class TestLotSize : AbstractTest
    {
        [Fact]
        public void TestALotSize()
        {
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            LotSize.LotSize lotSize = new LotSize.LotSize(new Quantity(6),
                dbMasterDataCache.M_ArticleGetAll()[0].GetId());
            foreach (var quantity in lotSize.GetLotSizes())
            {
                Assert.True(quantity.GetValue() == TestConfiguration.LotSize, $"Quantity ({quantity}) is not correct.");
            }
        }
    }
}