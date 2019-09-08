using System.IO;
using System.Text;
using Xunit;
using Zpp.DbCache;
using Zpp.Test.Configuration;
using Zpp.Utils;

namespace Zpp.Test.Ordergraphs.GanttChart
{
    public class TestGanttChart : AbstractTest
    {
        public TestGanttChart(): base(false)
        {
            
        }

        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_1_LOTSIZE_1)]
        [InlineData(TestConfigurationFileNames.DESK_COP_1_LOT_ORDER_QUANTITY)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_CONCURRENT_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_5_SEQUENTIALLY_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_1_LOTSIZE_1)]
        public void TestGanttChartBar(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);
            
            string orderGraphAsGanttChartFile =
                $"../../../Test/Ordergraphs/GanttChart/gantt_chart_{TestConfiguration.Name}.json";
            
            IDbMasterDataCache dbMasterDataCache = new DbMasterDataCache(ProductionDomainContext);
            IDbTransactionData dbTransactionData =
                new DbTransactionData(ProductionDomainContext, dbMasterDataCache);

            
            string actualGanttChart = new GraphicalRepresentation.GanttChart(dbTransactionData.ProductionOrderOperationGetAll(), dbMasterDataCache).ToString();
            // create initial file, if it doesn't exists (must be committed then)
            if (File.Exists(orderGraphAsGanttChartFile) == false)
            {
                File.WriteAllText(orderGraphAsGanttChartFile, actualGanttChart, Encoding.UTF8);
            }
            
            string expectedGanttChart = File.ReadAllText(orderGraphAsGanttChartFile, Encoding.UTF8);
            
            bool ganttChartHasNotChanged =
                expectedGanttChart.Equals(actualGanttChart);
            // for debugging: write the changed graphs to files
            if (ganttChartHasNotChanged == false)
            {
                File.WriteAllText(orderGraphAsGanttChartFile, actualGanttChart, Encoding.UTF8);
            }
            
            if (Constants.IsWindows)
            {
                Assert.True(ganttChartHasNotChanged, "Ganttchart has changed.");
                // Assert.True(orderGraphWithIdsHasNotChanged,"OrderGraph with ids has changed.");
            }
            else
            {
                // On linux the graph is always different so the test would always fail here.
                Assert.True(true);
            }

        }
        
        private void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);

            MrpRun.MrpRun.RunMrp(ProductionDomainContext);
        }
    }
}