using System;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Xunit;

namespace Zpp.Test
{
    public class TestPerformance : AbstractTest
    {
        private const int ORDER_QUANTITY = 1;
        private const int MAX_TIME_FOR_MRP_RUN = 60;

        public TestPerformance()
        {
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, ORDER_QUANTITY);
        }
    
        [Fact]
        public void TestMaxTimeForMrpRunIsNotExceeded()
        {
            DateTime startTime = DateTime.UtcNow;

            MrpRun.RunMrp(ProductionDomainContext);

            DateTime endTime = DateTime.UtcNow;
            Assert.True((endTime - startTime).TotalMilliseconds / 1000 < MAX_TIME_FOR_MRP_RUN,
                $"MrpRun for example use case ({ORDER_QUANTITY} customerOrder) " +
                $"takes longer than {MAX_TIME_FOR_MRP_RUN} seconds");
        }
    }
}