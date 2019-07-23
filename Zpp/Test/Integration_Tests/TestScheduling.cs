using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;
using Zpp.Utils;

namespace Zpp.Test
{
    public class TestScheduling : AbstractTest
    {
        private const int ORDER_QUANTITY = 6;
        private const int DEFAULT_LOTSIZE = 2;

        public TestScheduling()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
            LotSize.LotSize.SetDefaultLotSize(new Quantity(DEFAULT_LOTSIZE));
        }

        [Fact(Skip = "not implemented yet")]
        public void TestBackwardScheduling()
        {
            MrpRun.RunMrp(ProductionDomainContext);

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            List<M_ArticleBom> rootArticles = dbMasterDataCache.M_ArticleBomGetRootArticles();

            foreach (var rootArticle in rootArticles)
            {
                // TODO: traverse the demandToProviders, not the articleTree
                // --> therefore fill the new T_ProviderToDemand table
                ArticleTree articleTree = new ArticleTree(rootArticle, dbTransactionData);


                // traverse tree and execute an action
                TreeTools<M_Article>.traverseDepthFirst(articleTree, article =>
                {
                    // TODO: check now the backward scheduling
                });
            }
        }

    }
}