using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Master40.SimulationCore.Helper;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
        [InlineData(testResultCtxString)]
        [InlineData(masterResultCtxString)]
        public void ResetResultsDB(string connectionString)
        
        {
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
            MasterDBInitializerTruck.DbInitialize(masterCtx);

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
        //[InlineData(SimulationType.None)]
        [InlineData(SimulationType.DefaultSetup, 0, 60)]
        [InlineData(SimulationType.DefaultSetupStack, 1, 60)]
        //[InlineData(SimulationType.BucketScope, 3, 120)]
        //[InlineData(SimulationType.BucketScope, 4, 150)]
        //[InlineData(SimulationType.BucketScope, 5, 180)]
        //[InlineData(SimulationType.BucketScope, 6, 240)]
        //[InlineData(SimulationType.BucketScope, 7, 300)]
        //[InlineData(SimulationType.BucketScope, 8, 360)]
        //[InlineData(SimulationType.BucketScope, 9, 420)]
        //[InlineData(SimulationType.BucketScope, 10, 480)]
        [InlineData(SimulationType.BucketScope, 2, 600)]
        [InlineData(SimulationType.BucketScope, 3, 720)]
        [InlineData(SimulationType.BucketScope, 4, 840)]
        [InlineData(SimulationType.BucketScope, 5, 960)]
        [InlineData(SimulationType.BucketScope, 6, 1080)]
        [InlineData(SimulationType.BucketScope, 7, 1200)]
        [InlineData(SimulationType.BucketScope, 8, 1320)]
        [InlineData(SimulationType.BucketScope, 9, 1440)]
        [InlineData(SimulationType.BucketScope, 10, 1920)]
        [InlineData(SimulationType.BucketScope, 11, 2400)]
        [InlineData(SimulationType.BucketScope, 12, 2880)]
        [InlineData(SimulationType.BucketScope, 99, Int32.MaxValue)]
        public async Task SystemTestAsync(SimulationType simulationType, int simNr, int maxBucketSize)
        {
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);
            var simContext = new AgentSimulation(DBContext: _masterDBContext, messageHub: new ConsoleHub());

            var simConfig = SimulationCore.Environment.Configuration.Create(args: new object[]
                                                {
                                                    // set ResultDBString and set SaveToDB true
                                                    new DBConnectionString(value: remoteResultCtxString)
                                                    , new SimulationId(value: 1)
                                                    , new SimulationNumber(value: simNr)
                                                    , new SimulationKind(value: simulationType) // implements the used behaviour, if None --> DefaultBehaviour
                                                    , new OrderArrivalRate(value: 0.025)
                                                    , new OrderQuantity(value: Int32.MaxValue)
                                                    , new TransitionFactor(value: 3)
                                                    , new EstimatedThroughPut(value: 1920)
                                                    , new DebugAgents(value: false)
                                                    , new DebugSystem(value: false)
                                                    , new KpiTimeSpan(value: 480)
                                                    , new MaxBucketSize(value: maxBucketSize)
                                                    , new Seed(value: 1337)
                                                    , new MinDeliveryTime(value: 1440)
                                                    , new MaxDeliveryTime(value: 2880)
                                                    , new TimePeriodForThroughputCalculation(value: 3840)
                                                    , new SettlingStart(value: 4320)
                                                    , new SimulationEnd(value: 40320)
                                                    , new WorkTimeDeviation(value: 0.2)
                                                    , new SaveToDB(value: true)
                                                });

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
                // AgentSimulation.Continuation(inbox: simContext.SimulationConfig.Inbox
                //                             , sim: simulation
                //                             , collectors: new List<IActorRef> { simContext.StorageCollector
                //                                                     , simContext.WorkCollector
                //                                                     , simContext.ContractCollector
                //                             });
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
            var config = Simulation.CLI.ArgumentConverter.ConfigurationConverter(_ctxResult, 2);
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
