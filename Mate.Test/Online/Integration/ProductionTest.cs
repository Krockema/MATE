using System;
using System.Collections.Generic;
using System.Linq;
using Akka.TestKit.Xunit;
using AkkaSim.Logging;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.Data.Initializer;
using Mate.DataCore.Nominal;
using Mate.DataCore.Nominal.Model;
using Mate.DataCore.ReportingModel;
using Mate.Production.CLI;
using Mate.Production.Core;
using Mate.Production.Core.Environment.Options;
using Newtonsoft.Json;
using NLog;
using Xunit;
using Xunit.Abstractions;

namespace Mate.Test.Online.Integration
{
    public class ProductionTest : TestKit
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private DataBase<MateProductionDb> _contextDataBase;
        private DataBase<MateResultDb> _resultContextDataBase;

        public ProductionTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _contextDataBase = Dbms.GetNewMateDataBase();
            _resultContextDataBase = Dbms.GetNewResultDataBase();

        }
        // TODO Add test if all Contract, Dispo and Production Agents finished (i.e. using ConsoleHub)
        /// <summary>
        /// Scenario:
        /// - producing 5 products
        /// - seed 1337 to test dynamic pegging
        /// - different models for resource, tools, operators and workers
        ///
        /// Result / Objects to test:
        /// - simulation finish
        /// - produce all 5 products
        /// - no overlapping jobs on machine (based one the written Kpi)
        /// </summary>
        /// <param name="uniqueSimNum"></param>
        /// <param name="orderQuantity"></param>
        /// <param name="resourceModelSize"></param>
        /// <param name="setupModelSize"></param>
        /// <param name="operatorModelSize"></param>
        /// <param name="numberOfWorkers"></param>
        /// <param name="secondResource"></param>
        ///
        [Theory(Skip = "Not working on Travis, maybe change with migration to github actions")]
        [InlineData(1, 5, ModelSize.Medium, ModelSize.Medium, ModelSize.None, 0, false)]
        [InlineData(2, 5, ModelSize.Medium, ModelSize.Medium, ModelSize.None, 2, false)]
        [InlineData(3, 5, ModelSize.Medium, ModelSize.Medium, ModelSize.Medium, 0, false)]
        [InlineData(4, 5, ModelSize.Medium, ModelSize.Medium, ModelSize.Small, 3, false)]
        
        [InlineData(5, 5, ModelSize.TestModel, ModelSize.Medium, ModelSize.Small, 0, false)]
        public void RunProduction(int uniqueSimNum, int orderQuantity, ModelSize resourceModelSize,
            ModelSize setupModelSize, ModelSize operatorModelSize, int numberOfWorkers, bool secondResource)
        {

            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            _testOutputHelper.WriteLine("DatabaseString: " + _contextDataBase.ConnectionString.Value);

            _testOutputHelper.WriteLine("ResultDatabaseString: " + _resultContextDataBase.ConnectionString.Value);
            //Handle this one in our Resource Model?
            MasterDBInitializerTruck.DbInitialize(_contextDataBase.DbContext, resourceModelSize, setupModelSize,
                operatorModelSize, numberOfWorkers, secondResource, false);
            _testOutputHelper.WriteLine("MateDB Initialization finished");
            ResultDBInitializerBasic.DbInitialize(_resultContextDataBase.DbContext);
            _testOutputHelper.WriteLine("ResultD Basic Initialization finished");
            var messageHub = new ConsoleHub();
            var simConfig = ArgumentConverter.ConfigurationConverter(_resultContextDataBase.DbContext, 1);
            simConfig.AddOption(new ResultsDbConnectionString(_resultContextDataBase.ConnectionString.Value));
            simConfig.ReplaceOption(new TimeToAdvance(new TimeSpan(0L)));
            simConfig.ReplaceOption(new KpiTimeSpan(240));
            simConfig.ReplaceOption(new DebugAgents(true));
            simConfig.ReplaceOption(new MinDeliveryTime(1440));
            simConfig.ReplaceOption(new MaxDeliveryTime(2880));
            simConfig.ReplaceOption(new TransitionFactor(3));
            simConfig.ReplaceOption(new SimulationKind(value: SimulationType.Default));
            simConfig.ReplaceOption(new OrderArrivalRate(value: 0.15));
            simConfig.ReplaceOption(new OrderQuantity(value: orderQuantity));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: 1920));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 2880));
            simConfig.ReplaceOption(new Seed(value: 1337));
            simConfig.ReplaceOption(new SettlingStart(value: 0));
            simConfig.ReplaceOption(new SimulationEnd(value: 4380));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: 480));
            simConfig.ReplaceOption(new SimulationNumber(value: uniqueSimNum));
            simConfig.ReplaceOption(new DebugSystem(value: true));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.0));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));

            var simContext = new AgentSimulation(dbName: "Test", messageHub: messageHub);
            _testOutputHelper.WriteLine("ArgumentConverter finished");

            var simulation = simContext.InitializeSimulation(configuration: simConfig).Result;
            _testOutputHelper.WriteLine("simContext.InitializeSimulation finished");

            var sim = simulation.RunAsync();
            _testOutputHelper.WriteLine("simulation.RunAsync() finished");
            Within(TimeSpan.FromSeconds(120), async () =>
            {
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
            }).Wait();
            
            var processedOrders =
                _resultContextDataBase.DbContext.Kpis
                    .Single(x => x.IsFinal.Equals(true) && x.Name.Equals("OrderProcessed")).Value;

            Assert.Equal(orderQuantity, processedOrders); 

            Assert.False(AnyOverlappingTaskItemsExistsOnOneMachine());
            
            foreach (var obj in messageHub.Logs)
            {
                dynamic guardChildCounter = JsonConvert.DeserializeObject(obj);
                Assert.Equal(0, int.Parse(guardChildCounter[1].Value));
            }
            
            _contextDataBase.DbContext.Dispose();
            _resultContextDataBase.DbContext.Dispose();


        }

        public bool AnyOverlappingTaskItemsExistsOnOneMachine()
        {
            var overlapping = false;
            var taskItems = _resultContextDataBase.DbContext.TaskItems;
            
            var resourceList = taskItems.Select(x => new {x.Resource}).Distinct().ToList();

            foreach (var resource in resourceList)
            {
                var resourceTasks = taskItems.Where(x => x.Resource.Equals(resource.Resource)).ToList();
                overlapping = CheckIfAnyTasksOverlapps(resourceTasks);

                if(overlapping) break;

            }

            return overlapping;

        }
        
        private bool CheckIfAnyTasksOverlapps(List<TaskItem> tasks)
        {
            bool overlap = false;
            foreach (var task in tasks)
            {
                foreach (var comparedTask in tasks)
                {
                    if (comparedTask.Id.Equals(task.Id)) continue;

                    overlap = task.Start < comparedTask.End && comparedTask.Start < task.End;

                    if (overlap)
                        break;
                }

                if (overlap)
                    break;
            }

            return overlap;
        }

    }
}
