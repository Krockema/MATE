using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Enums;
using Master40.Simulation.CLI;
using Master40.SimulationCore;
using Master40.SimulationCore.Environment.Options;
using Master40.XUnitTest.Preparations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB;
using Xunit;
using Zpp.Utils;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class SimulationSystem : TestKit
    {
        private string localresultdb;
        private string productionDbConnectionString;
        private ProductionDomainContext _context;
        private ResultContext _resultContext;
        private int simNr = 999;

        public SimulationSystem()
        {
            productionDbConnectionString = Dbms.GetConnectionString();
            localresultdb = Dbms.GetResultConnectionString();
            _context = ProductionDomainContext.GetContext(productionDbConnectionString);
            _resultContext = ResultContext.GetContext(localresultdb);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            //MasterDbInitializerTable.DbInitialize(_masterDBContext);
            MasterDBInitializerTruck.DbInitialize(context: _context);

            _resultContext.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(context: _resultContext);

        }


        [Fact]
        public async Task SystemTestAsync()
        {
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);

            var simContext = new AgentSimulation(DBContext: _context, messageHub: new ConsoleHub());

            var simConfig = SimulationCore.Environment.Configuration.Create(args: new object[]
                                                {
                                                    // set ResultDBString and set SaveToDB true
                                                    new DBConnectionString(value: localresultdb)
                                                    , new SimulationId(value: 1)
                                                    , new SimulationNumber(value: simNr)
                                                    , new SimulationKind(value: SimulationType.None) // implements the used behaviour, if None --> DefaultBehaviour
                                                    , new OrderArrivalRate(value: 0.025)
                                                    , new OrderQuantity(value: 1)
                                                    , new TransitionFactor(value: 3)
                                                    , new EstimatedThroughPut(value: 600)
                                                    , new DebugAgents(value: true)
                                                    , new DebugSystem(value: false)
                                                    , new KpiTimeSpan(value: 480)
                                                    , new Seed(value: 1337)
                                                    , new MinDeliveryTime(value: 1160)
                                                    , new MaxDeliveryTime(value: 1600)
                                                    , new TimePeriodForThrougputCalculation(value: 3840)
                                                    , new SettlingStart(value: 2880)
                                                    , new SimulationEnd(value: 20160)
                                                    , new WorkTimeDeviation(value: 0.2)
                                                    , new SaveToDB(value: false)
                                                });

            var simulation = await simContext.InitializeSimulation(configuration: simConfig);

            emtpyResultDBbySimulationNumber(simNr: simConfig.GetOption<SimulationNumber>());


            var simFinished = false;
            if (simulation.IsReady())
            {
                // Start simulation
                var sim = simulation.RunAsync();

                AgentSimulation.Continuation(inbox: simContext.SimulationConfig.Inbox
                                            , sim: simulation
                                            , collectors: new List<IActorRef> { simContext.StorageCollector
                                                                    , simContext.WorkCollector
                                                                    , simContext.ContractCollector
                                            });
                await sim;
                // set for Assert 
                simFinished = true;
            }

            Assert.True(condition: simFinished);
        }

        private void emtpyResultDBbySimulationNumber(SimulationNumber simNr)
        {
            var _simNr = simNr;
            using (_resultContext)
            {
                _resultContext.RemoveRange(entities: _resultContext.SimulationOperations.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _resultContext.RemoveRange(entities: _resultContext.Kpis.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _resultContext.RemoveRange(entities: _resultContext.StockExchanges.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _resultContext.SaveChanges();
            }
        }
    }
}
