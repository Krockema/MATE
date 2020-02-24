using System;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Enums;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLitePCL;
using Xunit;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class SimulationSystem : TestKit
    {
        private string localresultdb = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string testResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string masterResultCtxString = "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true";

        ProductionDomainContext _ctx = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
                                                            .UseInMemoryDatabase(databaseName: "InMemoryDB")
                                                            .Options);

        ProductionDomainContext _masterDBContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer(connectionString: "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        private ResultContext _ctxResult = ResultContext.GetContext(resultCon:
            "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true");

        // new ResultContext(options: new DbContextOptionsBuilder<ResultContext>()
        // .UseInMemoryDatabase(databaseName: "InMemoryResults")
        // .Options);
        // 
        public SimulationSystem()
        {
            // _masterDBContext.Database.EnsureDeleted();
            // _masterDBContext.Database.EnsureCreated();
            // //MasterDbInitializerTable.DbInitialize(_masterDBContext);
            // MasterDBInitializerTruck.DbInitialize(context: _masterDBContext);

            // _ctxResult.Database.EnsureDeleted();
            // _ctxResult.Database.EnsureCreated();
            // ResultDBInitializerBasic.DbInitialize(context: _ctxResult);

        }

        //[Fact(Skip = "manual test")]
        [Theory]
        [InlineData(testResultCtxString)] 
        [InlineData(masterResultCtxString)]
        public void ResetResultsDB(string connectionString)
        
        {
            ResultContext masterResults = ResultContext.GetContext(resultCon: connectionString);
            masterResults.Database.EnsureDeleted();
            masterResults.Database.EnsureCreated();

            _masterDBContext.Database.EnsureDeleted();
            _masterDBContext.Database.EnsureCreated();
            //MasterDbInitializerTable.DbInitialize(_masterDBContext);
            MasterDBInitializerTruck.DbInitialize(context: _masterDBContext);
        }

        [Theory]
        //[InlineData(SimulationType.DefaultSetup, 100, Int32.MaxValue, 1920, 169)]
        //[InlineData(SimulationType.DefaultSetupStack, 1, Int32.MaxValue, 1920, 169)]
        [InlineData(SimulationType.BucketScope, 1, 960, 1920, 169)]
        [InlineData(SimulationType.BucketScope, 3, 960, 1960, 169)]
        [InlineData(SimulationType.BucketScope, 4, 960, 1960, 169)]
        [InlineData(SimulationType.BucketScope, 5, 960, 1960, 169)]
        //[InlineData(SimulationType.BucketScope, 4, Int32.MaxValue, 480, 169)]
        //[InlineData(SimulationType.BucketScope, 5, Int32.MaxValue, 560, 169)]
        //[InlineData(SimulationType.BucketScope, 6, Int32.MaxValue, 640, 169)]
        //[InlineData(SimulationType.BucketScope, 7, Int32.MaxValue, 720, 169)]
        //[InlineData(SimulationType.BucketScope, 8, Int32.MaxValue, 800, 169)]
        //[InlineData(SimulationType.BucketScope, 9, Int32.MaxValue, 880, 169)]
        //[InlineData(SimulationType.BucketScope, 10, Int32.MaxValue, 960, 169)]
        //[InlineData(SimulationType.BucketScope, 11, Int32.MaxValue, 1040, 169)]
        //[InlineData(SimulationType.BucketScope, 12, Int32.MaxValue, 1120, 169)]
        //[InlineData(SimulationType.BucketScope, 13, Int32.MaxValue, 1200, 169)]
        //[InlineData(SimulationType.BucketScope, 14, Int32.MaxValue, 1280, 169)]
        //[InlineData(SimulationType.BucketScope, 15, Int32.MaxValue, 1360, 169)]
        //[InlineData(SimulationType.BucketScope, 16, Int32.MaxValue, 1440, 169)]
        //[InlineData(SimulationType.BucketScope, 17, Int32.MaxValue, 1520, 169)]
        //[InlineData(SimulationType.BucketScope, 18, Int32.MaxValue, 1600, 169)]
        //[InlineData(SimulationType.BucketScope, 19, Int32.MaxValue, 1680, 169)]
        //[InlineData(SimulationType.BucketScope, 20, Int32.MaxValue, 1760, 169)]
        //[InlineData(SimulationType.BucketScope, 21, Int32.MaxValue, 1840, 169)]
        //[InlineData(SimulationType.BucketScope, 22, Int32.MaxValue, 1920, 169)]
        //[InlineData(SimulationType.BucketScope, 23, Int32.MaxValue, 2000, 169)]
        //[InlineData(SimulationType.BucketScope, 24, Int32.MaxValue, 2080, 169)]
        //[InlineData(SimulationType.BucketScope, 25, Int32.MaxValue, 2160, 169)]
        //[InlineData(SimulationType.BucketScope, 26, Int32.MaxValue, 2240, 169)]
        //[InlineData(SimulationType.BucketScope, 27, Int32.MaxValue, 2320, 169)]
        //[InlineData(SimulationType.BucketScope, 28, Int32.MaxValue, 2400, 169)]
        //[InlineData(SimulationType.BucketScope, 29, Int32.MaxValue, 2480, 169)]
        //[InlineData(SimulationType.BucketScope, 30, Int32.MaxValue, 2560, 169)]
        //[InlineData(SimulationType.BucketScope, 31, Int32.MaxValue, 2640, 169)]
        //[InlineData(SimulationType.BucketScope, 32, Int32.MaxValue, 2720, 169)]
        //[InlineData(SimulationType.BucketScope, 33, Int32.MaxValue, 2800, 169)]
        //[InlineData(SimulationType.BucketScope, 34, Int32.MaxValue, 2880, 169)]

        public async Task SystemTestAsync(SimulationType simulationType, int simNr, int maxBucketSize, long throughput, int seed)
        {
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);
            var simContext = new AgentSimulation(DBContext: _masterDBContext, messageHub: new ConsoleHub());

            var simConfig = SimulationCore.Environment.Configuration.Create(args: new object[]
                                                {
                                                    // set ResultDBString and set SaveToDB true
                                                    new DBConnectionString(value: localresultdb)
                                                    , new SimulationId(value: 1)
                                                    , new SimulationNumber(value: simNr)
                                                    , new SimulationKind(value: simulationType) // implements the used behaviour, if None --> DefaultBehaviour
                                                    , new OrderArrivalRate(value: 0.025)
                                                    , new OrderQuantity(value: Int32.MaxValue)
                                                    , new TransitionFactor(value: 3)
                                                    , new EstimatedThroughPut(value: throughput)
                                                    , new DebugAgents(value: false)
                                                    , new DebugSystem(value: false)
                                                    , new KpiTimeSpan(value: 480)
                                                    , new MaxBucketSize(value: maxBucketSize)
                                                    , new Seed(value: seed)
                                                    , new MinDeliveryTime(value: 1440)
                                                    , new MaxDeliveryTime(value: 2880)
                                                    , new TimePeriodForThrougputCalculation(value: 3840)
                                                    , new SettlingStart(value: 4320)
                                                    , new SimulationEnd(value: 40360)
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

                AgentSimulation.Continuation(inbox: simContext.SimulationConfig.Inbox
                                            , sim: simulation
                                            , collectors: new List<IActorRef> { simContext.StorageCollector
                                                                    , simContext.WorkCollector
                                                                    , simContext.ContractCollector
                                            });
                await sim;
            }

            Assert.True(condition: simWasReady);
        }

        private void emtpyResultDBbySimulationNumber(SimulationNumber simNr)
        {
            var _simNr = simNr;
            using (_ctxResult)
            {
                _ctxResult.RemoveRange(entities: _ctxResult.SimulationJobs.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(entities: _ctxResult.Kpis.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(entities: _ctxResult.StockExchanges.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.SaveChanges();
            }
        }

    }
}
