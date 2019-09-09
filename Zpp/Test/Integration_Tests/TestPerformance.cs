using System;
using Xunit;
using Zpp.Mrp;

namespace Zpp.Test.Integration_Tests
{
    public class TestPerformance : AbstractTest
    {
        
        private const int MAX_TIME_FOR_MRP_RUN = 90;

        public TestPerformance() : base(initDefaultTestConfig: true)
        {

        }
    
        [Fact]
        public void TestMaxTimeForMrpRunIsNotExceeded()
        {
            DateTime startTime = DateTime.UtcNow;

            MrpRun.Start(ProductionDomainContext);

            DateTime endTime = DateTime.UtcNow;
            double neededTime = (endTime - startTime).TotalMilliseconds / 1000;
            Assert.True( neededTime < MAX_TIME_FOR_MRP_RUN,
                $"MrpRun for example use case ({TestConfiguration.Name}) " +
                $"takes longer than {MAX_TIME_FOR_MRP_RUN} seconds: {neededTime}");
        }
    }
}