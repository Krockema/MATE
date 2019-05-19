using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;

namespace Zpp.Test
{
    public class IntegrationTest : AbstractTest
    {
        public IntegrationTest()
        {
            // TODO: orderQuantity should be set to higherValue (from simConfigs)
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1,
                true, 1);
        }

        [Fact]
        public void testMrpRun()
        {
            IDbCache dbCache = new DbCache(ProductionDomainContext);
            Assert.True(dbCache.T_DemandsGetAll().Count==1, "No demands are available.");
            MrpRun.runMrp(dbCache);
        }
    }
}