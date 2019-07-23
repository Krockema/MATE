using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;

namespace Zpp.Test
{
    public class TestOrderNet : AbstractTest
    {
        private const int ORDER_QUANTITY = 1;

        public TestOrderNet()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }
    
        /**
         * Verifies, that the orderNet
         * - can be build up from DemandToProvider+ProviderToDemand table
         * - is a top down graph
         */
        [Fact]
        public void TestOrderNetBuildUp()
        {
            MrpRun.RunMrp(ProductionDomainContext);

            // build it up

            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);
            IGraph<INode> orderGraph = new OrderGraph(dbTransactionData);

            Assert.True(orderGraph.GetAllToNodes().Count > 0,
                "There are no toNodes in the orderGraph.");

            int sumDemandToProviderAndProviderToDemand =
                dbTransactionData.DemandToProviderGetAll().Count() +
                dbTransactionData.ProviderToDemandGetAll().Count();

            Assert.True(sumDemandToProviderAndProviderToDemand == orderGraph.CountEdges(),
                $"Should be equal size: sumDemandToProviderAndProviderToDemand " +
                $"{sumDemandToProviderAndProviderToDemand} and  sumValuesOfOrderGraph {orderGraph.CountEdges()}");
        }
    }
}