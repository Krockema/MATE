using System;
using System.Diagnostics;
using Master40.SimulationMrp;
using Master40.XUnitTest.Zpp.Configuration;
using Xunit;
using Zpp;
using Zpp.Util;
using Zpp.Util.Performance;
using Zpp.Utils;

namespace Master40.XUnitTest.Zpp.Integration_Tests
{
    public class TestPerformance : AbstractTest
    {
        public TestPerformance() : base(initDefaultTestConfig: false)
        {
        }

        private void InitThisTest(string testConfiguration)
        {
            InitTestScenario(testConfiguration);
        }


        [Theory]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_2_LOTSIZE_2)]
        public void TestMaxTimeForMrpRunIsNotExceeded(string testConfigurationFileName)
        {
            const int MAX_TIME_FOR_MRP_RUN = 90;

            InitThisTest(testConfigurationFileName);

            DateTime startTime = DateTime.UtcNow;

            IZppSimulator zppSimulator = new Master40.SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartTestCycle();

            DateTime endTime = DateTime.UtcNow;
            double neededTime = (endTime - startTime).TotalMilliseconds / 1000;
            Assert.True(neededTime < MAX_TIME_FOR_MRP_RUN,
                $"MrpRun for example use case ({TestConfiguration.Name}) " +
                $"takes longer than {MAX_TIME_FOR_MRP_RUN} seconds: {neededTime}");
        }

        [Theory]
        // [InlineData(TestConfigurationFileNames.DESK_COP_2_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_5_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.DESK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_100_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_INTERVAL_20160_COP_100_LOTSIZE_2)]
        public void TestPerformanceStudyWithoutDbPersist(string testConfigurationFileName)
        {
            ExecutePerformanceStudy(testConfigurationFileName, false);
        }
        
        private void ExecutePerformanceStudy(string testConfigurationFileName, bool shouldPersist)
        {
            PerformanceMonitor performanceMonitor = new PerformanceMonitor(InstanceToTrack.Global);
            // int maxPossibleCops = int.MaxValue / 100;
            int maxPossibleCops = 1000;
            
            ZppConfiguration.CacheManager.ReadInTestConfiguration(testConfigurationFileName);
            ICentralPlanningConfiguration testConfiguration =
               ZppConfiguration.CacheManager.GetTestConfiguration();
            int customerOrderCount = ZppConfiguration.CacheManager.GetTestConfiguration()
                .CustomerOrderPartQuantity;
            int customerOrderCountOriginal = customerOrderCount;
            int elapsedMinutes = 0;
            int cycles = testConfiguration.SimulationMaximumDuration /
                         testConfiguration.SimulationInterval;

            string performanceLogLastCycles = "[";
            // n cycles here each cycle create & plan configured CustomerOrderPart 
            while (customerOrderCount <= maxPossibleCops && elapsedMinutes < 5)
            {
                InitThisTest(testConfigurationFileName);
                
                IZppSimulator zppSimulator = new global::Master40.SimulationMrp.impl.ZppSimulator();
                performanceMonitor.Start();
                
                zppSimulator.StartPerformanceStudy(shouldPersist);
                performanceMonitor.Stop();
                if (performanceLogLastCycles.Length>1)
                {
                    performanceLogLastCycles += ",";
                }
                performanceLogLastCycles += "{" + performanceMonitor.ToString();
                long currentMemoryUsage = Process.GetCurrentProcess().WorkingSet64;
                performanceLogLastCycles +=
                    $"\"CurrentMemoryUsage\": \"{currentMemoryUsage}\"" +
                    Environment.NewLine;
                performanceLogLastCycles += "}" + Environment.NewLine;
                
                customerOrderCount += customerOrderCountOriginal;
                testConfiguration.CustomerOrderPartQuantity = customerOrderCount;
            }
            // just for correct log name
            customerOrderCount -= customerOrderCountOriginal;
            testConfiguration.CustomerOrderPartQuantity = customerOrderCount;

            performanceLogLastCycles += "]";
            string logType = $"_{testConfiguration.Name}_cycles_{cycles}_COs_{testConfiguration.CustomerOrderPartQuantity}_lastCycles";
            ;
            DebuggingTools.WritePerformanceLog(performanceLogLastCycles, logType);
        }

        /**
         * without confirmations to compare it with performanceStudy (which includes confirmations)
         */
        [Theory]
        [InlineData(TestConfigurationFileNames.DESK_COP_2_LOTSIZE_2)]
        [InlineData(TestConfigurationFileNames.TRUCK_COP_2_LOTSIZE_2)]
        // [InlineData(TestConfigurationFileNames.TRUCK_COP_500_LOTSIZE_2)]
        public void TestMultipleTestCycles(string testConfigurationFileName)
        {
            InitThisTest(testConfigurationFileName);

            IZppSimulator zppSimulator = new SimulationMrp.impl.ZppSimulator();
            zppSimulator.StartMultipleTestCycles();
        }

        [Fact]
        public void TestCpuCycles()
        {
            PerformanceMonitor performanceMonitor = new PerformanceMonitor(InstanceToTrack.Global);
            performanceMonitor.Start();
            System.Threading.Thread.Sleep(1000);
            performanceMonitor.Stop();
            Assert.True(performanceMonitor.GetMeasuredCpuCycles() < 3000000);
        }
        
    }
}