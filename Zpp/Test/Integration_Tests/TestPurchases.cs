using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;

namespace Zpp.Test
{
    public class TestPurchases : AbstractTest
    {
        private const int ORDER_QUANTITY = 1;

        public TestPurchases()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }
        
        [Fact]
        public void TestPurchaseQuantityIsAVielfachesOfPacksize()
        {
            // TODO: Vielfaches in method name and assert

            MrpRun.RunMrp(ProductionDomainContext);

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData persistedTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            List<T_PurchaseOrderPart> tPurchaseOrderParts = persistedTransactionData
                .PurchaseOrderPartGetAll().GetAllAs<T_PurchaseOrderPart>();

            foreach (var tPurchaseOrderPart in tPurchaseOrderParts)
            {
                M_ArticleToBusinessPartner articleToBusinessPartner =
                    dbMasterDataCache.M_ArticleToBusinessPartnerGetAllByArticleId(
                        new Id(tPurchaseOrderPart.ArticleId))[0];
                Quantity multiplier = new Quantity(1);
                while (multiplier.GetValue() * articleToBusinessPartner.PackSize <
                       tPurchaseOrderPart.Quantity)
                {
                    multiplier.IncrementBy(new Quantity(1));
                }

                Quantity expectedPurchaseQuantity = new Quantity(multiplier.GetValue() *
                                                                 articleToBusinessPartner.PackSize);
                Assert.True(tPurchaseOrderPart.GetQuantity().Equals(expectedPurchaseQuantity),
                    $"Quantity of PurchaseOrderPart ({tPurchaseOrderPart}) ist not a Vielfaches of packSize.");
            }
        }
        
    }
}