using Master40.DB.Data.WrappersForPrimitives;
using Master40.XUnitTest.Zpp.Integration_Tests;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.Mrp2.impl.Mrp1.impl.LotSize.Impl;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestLotSize : AbstractTest
    {
        [Fact]
        public void TestALotSize()
        {
            IDbMasterDataCache dbMasterDataCache =
            ZppConfiguration.CacheManager.GetMasterDataCache();

            LotSize lotSize = new LotSize(new Quantity(6),
                dbMasterDataCache.M_ArticleGetAll()[0].GetId());
            foreach (var quantity in lotSize.GetLotSizes())
            {
                Assert.True(quantity.GetValue() == TestConfiguration.LotSize, $"Quantity ({quantity}) is not correct.");
            }
        }
    }
}