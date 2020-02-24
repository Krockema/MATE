using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Master40.DB.Data.Context;
using Master40.DB.Nominal;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using Microsoft.EntityFrameworkCore;

namespace Master40.XUnitTest.SimulationEnvironment
{
    [SimpleJob(RunStrategy.ColdStart, targetCount: 1)]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [RPlotExporter]
    public class BenchmarksOrderArrivalTest
    {
        private string localresultdb = "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true";

        private ProductionDomainContext _masterDBContext = new ProductionDomainContext(options: new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer(connectionString: "Server=(localdb)\\mssqllocaldb;Database=TestContext;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);
        private ResultContext _ctxResult = ResultContext.GetContext(resultCon:
            "Server=(localdb)\\mssqllocaldb;Database=TestResultContext;Trusted_Connection=True;MultipleActiveResultSets=true");

        [Params(SimulationType.DefaultSetup, SimulationType.DefaultSetupStack, SimulationType.BucketScope)]
        public SimulationType SimulationType;
        
        [Params(0.0200,0.0210, 0.0220,0.0230,0.0240, 0.0250, 0.0260, 0.0270, 0.0280, 0.0290, 0.0300)]
        public double OrderArrivalRate;

        [Params(960)]
        public int MaxBucketSize;

        private static int simNr = 1;

        [Benchmark]
        public async Task BenchmarksOrderArrival()
        {
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);
            var simContext = new AgentSimulation(DBContext: _masterDBContext, messageHub: new ConsoleHub());

            var simConfig = SimulationCore.Environment.Configuration.Create(args: new object[]
            {
                // set ResultDBString and set SaveToDB true
                new DBConnectionString(value: localresultdb), new SimulationId(value: 1), new SimulationNumber(value: simNr++),
                new SimulationKind(value: SimulationType) // implements the used behaviour, if None --> DefaultBehaviour
                ,
                new OrderArrivalRate(value: OrderArrivalRate), new OrderQuantity(value: Int32.MaxValue),
                new TransitionFactor(value: 3), new EstimatedThroughPut(value: 1920), new DebugAgents(value: false),
                new DebugSystem(value: false), new KpiTimeSpan(value: 480), new MaxBucketSize(value: MaxBucketSize),
                new Seed(value: 1337), new MinDeliveryTime(value: 1440), new MaxDeliveryTime(value: 2880),
                new TimePeriodForThroughputCalculation(value: 3840), new SettlingStart(value: 4320),
                new SimulationEnd(value: 20160), new WorkTimeDeviation(value: 0.2), new SaveToDB(value: false)
            });

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            // emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());


            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();

                simContext.StateManager.ContinueExecution(simulation);
                sim.Wait();
            }
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