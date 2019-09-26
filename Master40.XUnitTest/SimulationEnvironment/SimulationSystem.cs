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
using Master40.DB.Data.Helper;
using Xunit;
using Zpp.Utils;

namespace Master40.XUnitTest.SimulationEnvironment
{
    public class SimulationSystem : TestKit
    {
        private DataBase<ProductionDomainContext> _dataBaseProduction;
        private DataBase<ResultContext> _dataBaseResult;
        private int simNr = 999;

        public SimulationSystem()
        {
            _dataBaseProduction = Dbms.GetNewDataBase();
            _dataBaseResult = Dbms.GetResultDataBase();
            _dataBaseProduction.DbContext.Database.EnsureDeleted();
            _dataBaseProduction.DbContext.Database.EnsureCreated();
            //MasterDbInitializerTable.DbInitialize(_masterDBContext);
            MasterDBInitializerTruck.DbInitialize(context: _dataBaseProduction.DbContext);

            
            ResultDBInitializerBasic.DbInitialize(context: _dataBaseResult.DbContext);

        }


        [Fact]
        public async Task SystemTestAsync()
        {
            //InMemoryContext.LoadData(source: _masterDBContext, target: _ctx);

            var simContext = new AgentSimulation(DBContext: _dataBaseProduction.DbContext, messageHub: new ConsoleHub());

            var simConfig = SimulationCore.Environment.Configuration.Create(args: new object[]
                                                {
                                                    // set ResultDBString and set SaveToDB true
                                                    new DBConnectionString(value: _dataBaseResult.ConnectionString.Value)
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
            using (_dataBaseResult.DbContext)
            {
                _dataBaseResult.DbContext.RemoveRange(entities: _dataBaseResult.DbContext.SimulationOperations.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _dataBaseResult.DbContext.RemoveRange(entities: _dataBaseResult.DbContext.Kpis.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _dataBaseResult.DbContext.RemoveRange(entities: _dataBaseResult.DbContext.StockExchanges.Where(predicate: a => a.SimulationNumber.Equals(_simNr.Value)));
                _dataBaseResult.DbContext.SaveChanges();
            }
        }
    }
}
