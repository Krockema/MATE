using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Mate.DataCore;
using Mate.DataCore.Nominal;
using Mate.Production.CLI;
using Mate.Production.Core;
using Mate.Production.Core.Environment.Options;

namespace Mate.Test.SimulationEnvironment
{
    [SimpleJob(RunStrategy.ColdStart, invocationCount: 1)]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [RPlotExporter]
    public class BenchmarksOrderArrivalTest
    {

        private readonly string TestMateDb = "Test" + DataBaseConfiguration.MateDb;
        private readonly string TestMateResultDb = "Test" + DataBaseConfiguration.MateResultDb;

        [Params(SimulationType.Default)]
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
            var simContext = new AgentSimulation(dbName: TestMateDb, messageHub: new LoggingHub());

            var simConfig = Mate.Production.Core.Environment.Configuration.Create(args: new object[]
            {
                // set ResultDBString and set SaveToDB true
                new ResultsDbConnectionString(value: Dbms.GetResultDataBase(TestMateResultDb).ConnectionString.Value), new SimulationId(value: 1), new SimulationNumber(value: simNr++),
                new SimulationKind(value: SimulationType) // implements the used behaviour, if None --> DefaultBehaviour
                ,
                new OrderArrivalRate(value: OrderArrivalRate), new OrderQuantity(value: Int32.MaxValue),
                new TransitionFactor(value: 3), new EstimatedThroughPut(value: 1920), new DebugAgents(value: false),
                new DebugSystem(value: false), new KpiTimeSpan(value: 480), new MaxBucketSize(value: MaxBucketSize),
                new Production.Core.Environment.Options.Seed(value: 1337), new MinDeliveryTime(value: 10), new MaxDeliveryTime(value: 15),
                new TimePeriodForThroughputCalculation(value: 3840), new SettlingStart(value: 4320),
                new SimulationEnd(value: 20160), new WorkTimeDeviation(value: 0.2), new SaveToDB(value: false)
            });

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            // emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());


            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();

                // simContext.StateManager.ContinueExecution(simulation);
                sim.Wait();
            }
        }
    }
}