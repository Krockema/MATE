using System.Collections.Generic;
using Akka.TestKit.Xunit;
using AkkaSim.Logging;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.DB.Nominal.Model;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using PriorityRule = Master40.DB.Nominal.PriorityRule;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class AgentSystem : TestKit
    {

        // local Context
        private const string masterCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string masterResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true";

        // local TEST Context
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testGeneratorCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestGeneratorContext;Trusted_Connection=True;MultipleActiveResultSets=true";

        // remote Context
        private const string remoteMasterCtxString = "Server=141.56.137.25,1433;Persist Security Info=False;User ID=SA;Password=123*Start#;Initial Catalog=Master40;MultipleActiveResultSets=true";
        private const string remoteResultCtxString = "Server=141.56.137.25,1433;Persist Security Info=False;User ID=SA;Password=123*Start#;Initial Catalog=Master40Result;MultipleActiveResultSets=true";

        private const string hangfireCtxString = "Server=141.56.137.25;Database=Hangfire;Persist Security Info=False;User ID=SA;Password=123*Start#;MultipleActiveResultSets=true";

        // only for local usage
        private const string GanttPlanCtxString = "SERVER=(localdb)\\MSSQLLocalDB;DATABASE=GanttPlanImportTestDB;Trusted_connection=Yes;UID=;PWD=;MultipleActiveResultSets=true";

        private ProductionDomainContext _masterDBContext = ProductionDomainContext.GetContext(remoteMasterCtxString);
        private ResultContext _ctxResult = ResultContext.GetContext(resultCon: testResultCtxString);

        //[Fact(Skip = "manual test")]
        [Theory]
        //[InlineData(remoteMasterCtxString, remoteResultCtxString)] 
        //[InlineData(masterCtxString, masterResultCtxString)]
        [InlineData(testCtxString, testResultCtxString, testGeneratorCtxString)]
        public void ResetResultsDB(string connectionString, string resultConnectionString, string generatorConnectionString = testGeneratorCtxString)
        {
            MasterDBContext masterCtx = MasterDBContext.GetContext(connectionString);
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, ModelSize.Medium, ModelSize.Medium, ModelSize.Small, 3, false, false);

            ResultContext results = ResultContext.GetContext(resultCon: resultConnectionString);
            results.Database.EnsureDeleted();
            results.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(results);

            DataGeneratorContext generatorCtx = DataGeneratorContext.GetContext(generatorConnectionString);
            generatorCtx.Database.EnsureDeleted();
            generatorCtx.Database.EnsureCreated();

        }

        // [Fact(Skip = "MANUAL USE ONLY --> to reset Remote DB")]
        [Fact]
        public void InitializeRemote()
        {
            ResultContext results = ResultContext.GetContext(resultCon: remoteResultCtxString);
            results.Database.EnsureDeleted();
            results.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(results);

            MasterDBContext masterCtx = MasterDBContext.GetContext(remoteMasterCtxString);
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, resourceModelSize: ModelSize.Small, setupModelSize: ModelSize.Small, ModelSize.Small, 3, false, false);

            HangfireDBContext dbContext = new HangfireDBContext(options: new DbContextOptionsBuilder<HangfireDBContext>()
                .UseSqlServer(connectionString: hangfireCtxString)
                .Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            HangfireDBInitializer.DbInitialize(context: dbContext);
        }


        [Fact(Skip = "MANUAL USE ONLY --> to reset Remote DB")]
        public void ClearHangfire()
        {
            HangfireDBContext dbContext = new HangfireDBContext(options: new DbContextOptionsBuilder<HangfireDBContext>()
                .UseSqlServer(connectionString: hangfireCtxString)
                .Options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            HangfireDBInitializer.DbInitialize(context: dbContext);
        }
        [Fact]
        public void SomethingToPlayWith()
        {
            var masterCtx = ProductionDomainContext.GetContext(testCtxString);
            var resources = masterCtx.Resources
                //.Where(x => x.Count == 1)
                // .Include(x => x.RequiresResourceSetups)
                //     .ThenInclude(x => x.ChildResource)
                // .Include(x => x.UsedInResourceSetups)
                //     .ThenInclude(x => x.ResourceCapability)
                .ToList(); // all Resources

            foreach (var resource in resources)
            {
                GetSetups(resource, masterCtx);
            }

        }

        private void GetSetups(M_Resource resource, ProductionDomainContext masterCtx)
        {
            if (!resource.IsPhysical)
                return;
            var setups = masterCtx.ResourceSetups
                .Include(x => x.ResourceCapabilityProvider)
                    .ThenInclude(x => x.ResourceCapability)
                .Include(x => x.Resource)
                .Where(x => x.ResourceId == resource.Id).ToList();

            System.Diagnostics.Debug.WriteLine($"Creating Resource: {resource.Name} with following setups...");
            foreach (var setup in setups)
            {
                System.Diagnostics.Debug.WriteLine($"{setup.Name} : {setup.ResourceCapabilityProvider.Name} : {setup.ResourceCapabilityProviderId}");
            }
        }


        // [InlineData(SimulationType.Default, 700, 480, 1920, 594, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 701, 480, 1920, 281, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 702, 480, 1920, 213, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 703, 480, 1920, 945, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 704, 480, 1920, 998, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 705, 480, 1920, 120, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 706, 480, 1920, 124, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 707, 480, 1920, 854, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 708, 480, 1920, 213, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]
        // [InlineData(SimulationType.Default, 709, 480, 1920, 325, ModelSize.Medium, ModelSize.Medium, 0.015, false, false)]

        public static IEnumerable<object[]> GetTestData()
        {
            var simNumber = 16000;
            var throughput = 1920;
            var seed = 594;

            for (int i = 0; i < 5; i++)
            {
                yield return new object[]
                {
                    SimulationType.Default, PriorityRule.LST, simNumber++, 960, throughput, seed++, ModelSize.Medium, ModelSize.Medium, 0.0153, false, false
                };
                //throughput += 100;
            }
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        [MemberData(nameof(GetTestData))]

        public async Task SystemTestAsync(SimulationType simulationType, PriorityRule priorityRule
            , int simNr, int maxBucketSize, long throughput, int seed
            , ModelSize resourceModelSize, ModelSize setupModelSize
            , double arrivalRate, bool distributeSetupsExponentially
            , bool createMeasurements = false, int numberOfValuesForPrediction = 4)
        {
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Trace, LogLevel.Trace);
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.PRIORITY, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.SCHEDULING, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.DISPOPRODRELATION, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.PROPOSAL, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.INITIALIZE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, CustomLogger.JOB, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.ENQUEUE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, CustomLogger.JOBSTATE, LogLevel.Warn, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace, LogLevel.Trace);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AKKA, LogLevel.Warn);

            //CreateMaster40Result
            var masterPlanResultContext = ResultContext.GetContext(testResultCtxString);
            /*masterPlanResultContext.Database.EnsureDeleted();
            masterPlanResultContext.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(masterPlanResultContext);
            */
            var masterCtx = ProductionDomainContext.GetContext(testCtxString);
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, resourceModelSize, setupModelSize, setupModelSize, 3, distributeSetupsExponentially, false);


            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);
            var simContext = new AgentSimulation(DBContext: masterCtx, messageHub: new ConsoleHub());
            var simConfig = Simulation.CLI.ArgumentConverter.ConfigurationConverter(masterPlanResultContext, 1);
            // update customized Items
            simConfig.AddOption(new DBConnectionString(testResultCtxString));
            simConfig.ReplaceOption(new TimeConstraintQueueLength(480));
            simConfig.ReplaceOption(new KpiTimeSpan(480));
            simConfig.ReplaceOption(new SimulationKind(value: simulationType));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: 1500));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new TimePeriodForThroughputCalculation(value: 1920));
            simConfig.ReplaceOption(new Seed(value: seed));
            simConfig.ReplaceOption(new SettlingStart(value: 2880));
            simConfig.ReplaceOption(new SimulationEnd(value: 40360));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: maxBucketSize));
            simConfig.ReplaceOption(new SimulationNumber(value: simNr));
            simConfig.ReplaceOption(new DebugSystem(value: false));
            simConfig.ReplaceOption(new DebugAgents(value: false));
            simConfig.ReplaceOption(new WorkTimeDeviation(0.0));
            simConfig.ReplaceOption(new MinDeliveryTime(1920));
            simConfig.ReplaceOption(new MaxDeliveryTime(2880));
            simConfig.ReplaceOption(new SimulationCore.Environment.Options.PriorityRule(priorityRule));
            simConfig.ReplaceOption(new CreateQualityData(createMeasurements));
            simConfig.ReplaceOption(new UsePredictedThroughput(numberOfValuesForPrediction));

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());

            var simWasReady = false;
            if (simulation.IsReady())
            {
                // set for Assert 
                simWasReady = true;
                // Start simulation
                var sim = simulation.RunAsync();
                simContext.StateManager.ContinueExecution(simulation);
                await sim;
            }

            Assert.True(condition: simWasReady);
        }

        [Fact(Skip = "Offline")]
        public void AggreteResults()
        {
            var _resultContext = ResultContext.GetContext(remoteResultCtxString);

            var aggregator = new ResultAggregator(_resultContext);
            aggregator.BuildResults(1);
        }


        [Fact]
        private void ArgumentConverter()
        {
            var numberOfArguments = _ctxResult.ConfigurationRelations.Count(x => x.Id == 1);
            var config = Simulation.CLI.ArgumentConverter.ConfigurationConverter(_ctxResult, 2);
            Assert.Equal(numberOfArguments + 1, config.Count());
        }

        private void emtpyResultDBbySimulationNumber(SimulationNumber simNr)
        {
            var _simNr = simNr;
            using (_ctxResult)
            {
                var itemsToRemove =
                    _ctxResult.SimulationJobs.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)).ToList();
                _ctxResult.RemoveRange(entities: itemsToRemove);
                _ctxResult.RemoveRange(entities: _ctxResult.Kpis.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(entities: _ctxResult.StockExchanges.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.SaveChanges();
            }
        }

    }
}
