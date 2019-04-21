using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;

namespace Zpp.Test
{
    public class IntegrationTest : AbstractTest
    {
        public IntegrationTest()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1,
                true);
        }

        [Fact]
        public void testBomExplosion()
        {
            // TODO
        }
    }
}