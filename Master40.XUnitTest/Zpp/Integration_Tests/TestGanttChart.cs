using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationMrp;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.GraphicalRepresentation;
using Zpp.GraphicalRepresentation.impl;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestGanttChart : AbstractTest
    {
        public TestGanttChart(): base(false)
        {
            
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        public void TestGanttChartBar(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IDbTransactionData dbTransactionData =
                global::Zpp.ZppConfiguration.CacheManager.ReloadTransactionData();

            IGanttChart ganttChart =
                new global::Zpp.GraphicalRepresentation.impl.GanttChart(dbTransactionData
                    .ProductionOrderOperationGetAll());
            string actualGanttChart = ganttChart.ToString();

            Assert.NotNull(actualGanttChart);
            
        }
        
        private void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);

            IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();
        }

        [Fact]
        public void TestDetermineFreeGroup()
        {
            //  first case overlapping
            Interval interval1 = new Interval(new Id(1), new DueTime(740), new DueTime(836));
            Interval interval2 = new Interval(new Id(2), new DueTime(736), new DueTime(836));
            Assert.True(interval1.IntersectsExclusive(interval2));
        }
    }
}