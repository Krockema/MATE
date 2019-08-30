using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;

namespace Master40.DB.Data.Initializer
{
    public static class ResultDBInitializerBasic
    {
        public static void DbInitialize(ResultContext context)
        {
            context.Database.EnsureCreated();

            if (context.SimulationConfigurations.Any())
            {
                return;   // DB has been seeded
            }

            var simConfigs = new List<SimulationConfiguration>();
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 1
                Name = "Lot 5, 24h, 24h, 0.2",
                Lotsize = 5,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.2

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 2
                Name = "Lot 10, 24h, 24h, 0.2",
                Lotsize = 10,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.2

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 3
                Name = "Lot 1, 8h, 8h, 0.2",
                Lotsize = 1,
                MaxCalculationTime = 480,
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 480,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.2

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 4
                Name = "Lot 1, 28h, 24h, 0.2",
                Lotsize = 1,
                MaxCalculationTime = 1680, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.2

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 5
                Name = "Lot 1, 24h, 24h, 0",
                Lotsize = 1,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.0

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 6
                Name = "Lot 1, 24h, 24h, 0.2",
                Lotsize = 1,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.2

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 7
                Name = "Lot 1, 24h, 24h, 0.4",
                Lotsize = 1,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.4

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 8
                Name = "decentral dev 0",
                Lotsize = 1,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.0

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 9
                Name = "decentral dev 0.2",
                Lotsize = 1,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.2

            });
            simConfigs.Add(item: new SimulationConfiguration()
            {
                //simconfigId = 10
                Name = "decentral dev 0.4",
                Lotsize = 1,
                MaxCalculationTime = 1440, // test  // 10080, // 7 days
                OrderQuantity = 600,
                Seed = 1340,
                ConsecutiveRuns = 1,
                OrderRate = 0.25, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 2880,
                WorkTimeDeviation = 0.4

            });

            simConfigs.Add(item: new SimulationConfiguration
            {
                Name = "Test config",
                Lotsize = 1,
                MaxCalculationTime = 480, // test  // 10080, // 7 days
                OrderQuantity = 550,
                Seed = 1338,
                ConsecutiveRuns = 1,
                OrderRate = 0.025, //0.25
                Time = 0,
                RecalculationTime = 1440,
                SimulationEndTime = 21000,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 0,
                WorkTimeDeviation = 0
            });

            context.SimulationConfigurations.AddRange(entities: simConfigs);
            context.SaveChanges();
        }
    }
}
