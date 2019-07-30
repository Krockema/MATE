using System;
using Akka.Actor;
using Akka.TestKit.Xunit;
using AkkaSim.Definitions;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.SimulationCore;
using Master40.XUnitTest.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Simulation.CLI;
using Master40.SimulationCore.Helper;
using Xunit;
using Master40.SimulationCore.Environment.Options;
using Master40.DB.Enums;

namespace Master40.XUnitTest.Agents
{
    public class SimulationSystem : TestKit
    {
        private string localresultdb = "Server=(localdb)\\mssqllocaldb;Database=Master40Results;Trusted_Connection=True;MultipleActiveResultSets=true";
        private int simNr = 999;

        ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                                                            .UseInMemoryDatabase(databaseName: "InMemoryDB")
                                                            .Options);
        
        /*ProductionDomainContext _masterDBContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);
        */
        ResultContext _ctxResult = new ResultContext(new DbContextOptionsBuilder<ResultContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryResults")
            .Options);
        
        public SimulationSystem()
        {
            _ctx.Database.EnsureCreated();
            //MasterDBInitializerMedium.DbInitialize(_ctx);
            //MasterDBInitializerSmall.DbInitialize(_ctx);
            MasterDBInitializerSimple.DbInitialize(_ctx);

            _ctxResult.Database.EnsureCreated();
            ResultDBInitializerBasic.DbInitialize(_ctxResult);
            // MasterDBInitializerLarge.DbInitialize(_ctx);
            //_productionDomainContext.Database.EnsureDeleted();
            //_productionDomainContext.Database.EnsureCreated();
            //MasterDBInitializerSimple.DbInitialize(_productionDomainContext);
        }


        [Fact]
        public async Task SystemTestAsync()
        {
            
            var simContext = new AgentSimulation(true, _ctx, new ConsoleHub());

            var simConfig = SimulationCore.Environment.Configuration.Create(new object[]
                                                {
                                                    // set ResultDBString and set SaveToDB true
                                                    new DBConnectionString(localresultdb)
                                                    , new SimulationId(1)
                                                    , new SimulationNumber(simNr)
                                                    , new SimulationKind(SimulationType.Decentral)
                                                    , new OrderArrivalRate(0.0275)
                                                    , new OrderQuantity(1)
                                                    , new EstimatedThroughPut(800)
                                                    , new DebugAgents(false)
                                                    , new DebugSystem(false)
                                                    , new KpiTimeSpan(480)
                                                    , new Seed(1337)
                                                    , new SettlingStart(2880)
                                                    , new SimulationEnd(20160)
                                                    , new WorkTimeDeviation(0.2)
                                                    , new SaveToDB(false)
                                                });
            // simConfig.OrderQuantity = 0;
   
            //var simModelConfig = new SimulationConfig(false, 480);
            var simulation = await simContext.InitializeSimulation(simConfig);

            emtpyResultDBbySimulationNumber(simConfig.GetOption<SimulationNumber>());


            // simulation.ActorSystem.EventStream.Subscribe(testProbe, typeof(DirectoryAgent.Instruction.CreateMachineAgents));

            var simWasReady = false;
            if (simulation.IsReady())
            {
                // set for Assert 
                simWasReady = true;
                // Start simulation
                var sim = simulation.RunAsync();

                AgentSimulation.Continuation(simContext.SimulationConfig.Inbox
                                            , simulation
                                            , new List<IActorRef> { simContext.StorageCollector
                                                                    , simContext.WorkCollector
                                                                    , simContext.ContractCollector
                                            });
                await sim;
            }
            
            Assert.True(simWasReady);
        }

        private void emtpyResultDBbySimulationNumber(SimulationNumber simNr)
        {
            var _simNr = simNr;
            using (_ctxResult)
            {
                _ctxResult.RemoveRange(_ctxResult.SimulationOperations.Where(a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(_ctxResult.Kpis.Where(a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.RemoveRange(_ctxResult.StockExchanges.Where(a => a.SimulationNumber.Equals(_simNr.Value)));
                _ctxResult.SaveChanges();
            }
        }
    }
}
