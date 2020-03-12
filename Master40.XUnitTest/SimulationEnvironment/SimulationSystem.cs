using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using System.Linq;
using System.Threading.Tasks;
using AkkaSim.Logging;
using Master40.SimulationCore.Helper;
using Microsoft.EntityFrameworkCore;
using NLog;
using Xunit;
using System;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class SimulationSystem : TestKit
    {

        // local Context
        private const string masterCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string masterResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true";
        
        // local TEST Context
        private const string testCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        
        // remote Context
        private const string remoteMasterCtxString = "Server=141.56.137.25,1433;Persist Security Info=False;User ID=SA;Password=123*Start#;Initial Catalog=Master40;MultipleActiveResultSets=true";
        private const string remoteResultCtxString = "Server=141.56.137.25,1433;Persist Security Info=False;User ID=SA;Password=123*Start#;Initial Catalog=Master40Result;MultipleActiveResultSets=true";

        private const string hangfireCtxString = "Server=141.56.137.25;Database=Hangfire;Persist Security Info=False;User ID=SA;Password=123*Start#;MultipleActiveResultSets=true";


        private ProductionDomainContext _masterDBContext = ProductionDomainContext.GetContext(remoteMasterCtxString);
        private ResultContext _ctxResult = ResultContext.GetContext(resultCon: remoteResultCtxString);

        // new ResultContext(options: new DbContextOptionsBuilder<ResultContext>()
        // .UseInMemoryDatabase(databaseName: "InMemoryResults")
        // .Options);
        // 
        public SimulationSystem()
        {

        }

        //[Fact(Skip = "manual test")]
        [Theory]
        //[InlineData(testResultCtxString)] 
        //[InlineData(masterResultCtxString)]
        [InlineData(remoteResultCtxString)]
        public void ResetResultsDB(string connectionString)
        
        {
            /*MasterDBContext masterCtx = MasterDBContext.GetContext(remoteMasterCtxString);
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, ModelSize.Medium, ModelSize.Medium, true);
            */
            ResultContext results = ResultContext.GetContext(resultCon: connectionString);
            results.Database.EnsureDeleted();
            results.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(results);
  
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
            MasterDBInitializerTruck.DbInitialize(masterCtx, resourceModelSize: ModelSize.Small, setupModelSize: ModelSize.Small);

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

        [Theory]
        //[InlineData(SimulationType.DefaultSetup, 1, Int32.MaxValue, 1920, 169, ModelSize.Small, ModelSize.Small)]//[InlineData(SimulationType.BucketScope, 53, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 13, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 14, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 15, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 16, Int32.MaxValue, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 17, Int32.MaxValue, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 18, Int32.MaxValue, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, true)]
        //[InlineData(SimulationType.DefaultSetupStack, 23, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, false)]
        //[InlineData(SimulationType.DefaultSetupStack, 24, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, false)]
        //[InlineData(SimulationType.DefaultSetupStack, 25, Int32.MaxValue, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, false)]
        //[InlineData(SimulationType.DefaultSetupStack, 26, Int32.MaxValue, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, false)]
        //[InlineData(SimulationType.DefaultSetupStack, 27, Int32.MaxValue, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, false)]
        //[InlineData(SimulationType.DefaultSetupStack, 28, Int32.MaxValue, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, false)]
        // [InlineData(SimulationType.BucketScope, 53, 960, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, true)]
        // [InlineData(SimulationType.BucketScope, 54, 960, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, true)]
        // [InlineData(SimulationType.BucketScope, 55, 960, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, true)]
        // [InlineData(SimulationType.BucketScope, 56, 960, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, true)]
        // [InlineData(SimulationType.BucketScope, 57, 960, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, true)]
        // [InlineData(SimulationType.BucketScope, 58, 960, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, true)]
        // [InlineData(SimulationType.BucketScope, 63, 960, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, false)]
        // [InlineData(SimulationType.BucketScope, 64, 960, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, false)]
        // [InlineData(SimulationType.BucketScope, 65, 960, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, false)]
        // [InlineData(SimulationType.BucketScope, 66, 960, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, false)]
        // [InlineData(SimulationType.BucketScope, 67, 960, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, false)]
        // [InlineData(SimulationType.BucketScope, 68, 960, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, false)]
        //[InlineData(SimulationType.BucketScope, 73, 480, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, true)]
        //[InlineData(SimulationType.BucketScope, 74, 480, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, true)]
        //[InlineData(SimulationType.BucketScope, 75, 480, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, true)]
        //[InlineData(SimulationType.BucketScope, 76, 480, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, true)]
        //[InlineData(SimulationType.BucketScope, 77, 480, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, true)]
        //[InlineData(SimulationType.BucketScope, 78, 480, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, true)]
        //[InlineData(SimulationType.BucketScope, 83, 480, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, false)]
        //[InlineData(SimulationType.BucketScope, 84, 480, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, false)]
        //[InlineData(SimulationType.BucketScope, 85, 480, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, false)]
        //[InlineData(SimulationType.BucketScope, 86, 480, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, false)]
        //[InlineData(SimulationType.BucketScope, 87, 480, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, false)]
        //[InlineData(SimulationType.BucketScope, 88, 480, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, false)]
        //[InlineData(SimulationType.BucketScope, 3, 960, 1920, 169)]
        // [InlineData(SimulationType.BucketScope, 153, 1440, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, true)]
        // [InlineData(SimulationType.BucketScope, 154, 1440, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, true)]
        // [InlineData(SimulationType.BucketScope, 155, 1440, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, true)]
        // [InlineData(SimulationType.BucketScope, 156, 1440, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, true)]
        // [InlineData(SimulationType.BucketScope, 157, 1440, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, true)]
        // [InlineData(SimulationType.BucketScope, 158, 1440, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, true)]
        [InlineData(SimulationType.BucketScope, 363, 480, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, false)]
        [InlineData(SimulationType.BucketScope, 364, 480, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, false)]
        [InlineData(SimulationType.BucketScope, 365, 480, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, false)]
        [InlineData(SimulationType.BucketScope, 366, 480, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, false)]
        [InlineData(SimulationType.BucketScope, 367, 480, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, false)]
        [InlineData(SimulationType.BucketScope, 368, 480, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, false)]
        // [InlineData(SimulationType.BucketScope, 373, 120, 1920, 169, ModelSize.Medium, ModelSize.Small, 0.025, false)]
        // [InlineData(SimulationType.BucketScope, 374, 120, 1920, 169, ModelSize.Medium, ModelSize.Medium, 0.025, false)]
        // [InlineData(SimulationType.BucketScope, 375, 120, 1920, 169, ModelSize.Medium, ModelSize.Large, 0.025, false)]
        // [InlineData(SimulationType.BucketScope, 376, 120, 1920, 169, ModelSize.Large, ModelSize.Small, 0.04, false)]
        // [InlineData(SimulationType.BucketScope, 377, 120, 1920, 169, ModelSize.Large, ModelSize.Medium, 0.04, false)]
        // [InlineData(SimulationType.BucketScope, 378, 120, 1920, 169, ModelSize.Large, ModelSize.Large, 0.04, false)]

        public async Task SystemTestAsync(SimulationType simulationType, int simNr, int maxBucketSize, long throughput, int seed, ModelSize resourceModelSize, ModelSize setupModelSize, double arrivalRate, bool distributeSetupsExponentially)
        {
            LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Info);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AKKA, LogLevel.Warn);
            
            var masterCtx = ProductionDomainContext.GetContext(testCtxString);
            masterCtx.Database.EnsureDeleted();
            masterCtx.Database.EnsureCreated();
            MasterDBInitializerTruck.DbInitialize(masterCtx, resourceModelSize, setupModelSize, distributeSetupsExponentially);

            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);
            var simContext = new AgentSimulation(DBContext: masterCtx, messageHub: new ConsoleHub());

            // LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Warn);
            // LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AGENTS, LogLevel.Info);
            var simConfig = Simulation.CLI.ArgumentConverter.ConfigurationConverter(_ctxResult, 1);
            // update customized Items
            simConfig.AddOption(new DBConnectionString(remoteResultCtxString));
            simConfig.ReplaceOption(new SimulationKind(value: simulationType));
            simConfig.ReplaceOption(new OrderArrivalRate(value: arrivalRate));
            simConfig.ReplaceOption(new OrderQuantity(value: int.MaxValue));
            simConfig.ReplaceOption(new EstimatedThroughPut(value: throughput));
            simConfig.ReplaceOption(new Seed(value: seed));
            simConfig.ReplaceOption(new SettlingStart(value: 4320));
            simConfig.ReplaceOption(new SimulationEnd(value: 20160));
            simConfig.ReplaceOption(new SaveToDB(value: true));
            simConfig.ReplaceOption(new MaxBucketSize(value: maxBucketSize));
            simConfig.ReplaceOption(new SimulationNumber(value: simNr));

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

        [Fact]
        public void AggreteResults()
        {
            var  _resultContext = ResultContext.GetContext(remoteResultCtxString);

            var aggregator = new ResultAggregator(_resultContext);
            aggregator.BuildResults(1);
        }
        
        
        
        
        
        [Fact]
        private void ArgumentConverter()
        {
            var numberOfArguments = _ctxResult.ConfigurationRelations.Count(x => x.Id == 1);
            var config = Simulation. CLI.ArgumentConverter.ConfigurationConverter(_ctxResult, 2);
            Assert.Equal(numberOfArguments +1, config.Count());
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
