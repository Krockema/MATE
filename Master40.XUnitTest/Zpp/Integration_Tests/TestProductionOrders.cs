using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Xunit;
using Zpp.DbCache;
using Zpp.Mrp;
using Zpp.Mrp.ProductionManagement.ProductionTypes;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestProductionOrders : AbstractTest
    {
        [Fact]
        public void TestProductionOrderBomIsACopyOfArticleBom()
        {
            if (global::Zpp.Configuration.Configuration.ProductionType.Equals(ProductionType.WorkshopProductionClassic))
            {
                Assert.True(true);
            }
            else
            {
                MrpRun.Start(ProductionDomainContext);
                IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
                IDbTransactionData dbTransactionData =
                    new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

                foreach (var productionOrderBom in dbTransactionData.ProductionOrderBomGetAll())
                {
                    T_ProductionOrderBom tProductionOrderBom =
                        (T_ProductionOrderBom)productionOrderBom.ToIDemand();
                    M_ArticleBom articleBom =
                        dbMasterDataCache.M_ArticleBomGetByArticleChildId(
                            new Id(tProductionOrderBom.ArticleChildId));
                    Assert.True(tProductionOrderBom.Quantity.Equals(articleBom.Quantity),
                        "Quantity of ProductionOrderBom does not equal the quantity from its articleBom.");
                }
            }

        }

        [Fact]
        public void TestProductionOrderOperationIsACopyOfM_Operation()
        {
            if (global::Zpp.Configuration.Configuration.ProductionType.Equals(ProductionType.WorkshopProductionClassic))
            {
                Assert.True(true);
            }
            else
            {

                MrpRun.Start(ProductionDomainContext);
                IDbMasterDataCache dbMasterDataCache =
                    new DbMasterDataCache(ProductionDomainContext);
                IDbTransactionData dbTransactionData =
                    new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

                foreach (var productionOrderOperation in dbTransactionData
                    .ProductionOrderOperationGetAll())
                {
                    T_ProductionOrderOperation tProductionOrderOperation =
                        productionOrderOperation.GetValue();
                    T_ProductionOrderBom aProductionOrderBom =
                        (T_ProductionOrderBom)dbTransactionData.GetAggregator()
                            .GetAnyProductionOrderBomByProductionOrderOperation(
                                productionOrderOperation).ToIDemand();
                    M_ArticleBom articleBom =
                        dbMasterDataCache.M_ArticleBomGetByArticleChildId(
                            new Id(aProductionOrderBom.ArticleChildId));
                    M_Operation mOperation =
                        dbMasterDataCache.M_OperationGetById(
                            new Id(articleBom.OperationId.GetValueOrDefault()));

                    string errorMessage =
                        "Property of ProductionOrderBom does not equal the one from its articleBom.";
                    Assert.True(
                        tProductionOrderOperation.HierarchyNumber.Equals(mOperation
                            .HierarchyNumber), errorMessage);
                    Assert.True(tProductionOrderOperation.Duration.Equals(mOperation.Duration),
                        errorMessage);
                    Assert.True(
                        tProductionOrderOperation.ResourceToolId.Equals(mOperation.ResourceToolId),
                        errorMessage);
                    Assert.True(
                        tProductionOrderOperation.ResourceSkillId.Equals(mOperation.ResourceSkillId),
                        errorMessage);
                }
            }
        }
    }
}