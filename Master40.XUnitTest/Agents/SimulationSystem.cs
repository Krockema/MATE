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
        ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                                                            .UseInMemoryDatabase(databaseName: "InMemoryDB")
                                                            .Options);

        ProductionDomainContext _masterDBContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        ResultContext _ctxResult = new ResultContext(new DbContextOptionsBuilder<ResultContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryResults")
            .Options);

        public SimulationSystem()
        {
            _ctx.Database.EnsureDeleted();
            _ctx.Database.EnsureCreated();
            //MasterDBInitializerMedium.DbInitialize(_ctx);
            MasterDBInitializerSmall.DbInitialize(_ctx);
            // MasterDBInitializerLarge.DbInitialize(_ctx);
            //_productionDomainContext.Database.EnsureDeleted();
            //_productionDomainContext.Database.EnsureCreated();
            //MasterDBInitializerLarge.DbInitialize(_productionDomainContext);
        }


        [Fact]
        public async Task SystemTestAsync()
        {
            var simConfig = SimulationCore.Environment.Configuration.Create(new object[]
                                                {
                                                    new DBConnectionString("")
                                                    , new SimulationId(1)
                                                    , new SimulationNumber(1)
                                                    , new SimulationKind(SimulationType.Decentral)
                                                    , new OrderArrivalRate(0.0275)
                                                    , new OrderQuantity(550)
                                                    , new EstimatedThroughPut(600)
                                                    , new DebugAgents(false)
                                                    , new DebugSystem(false)
                                                    , new KpiTimeSpan(480)
                                                    , new Seed(1337)
                                                    , new MinDeliveryTime(1160)
                                                    , new MaxDeliveryTime(1600)
                                                    , new SettlingStart(2880)
                                                    , new SimulationEnd(20160)
                                                    , new WorkTimeDeviation(0.2)
                                                    , new SaveToDB(false)
                                                });
            // simConfig.OrderQuantity = 0;
            var simContext = new AgentSimulation(false, _ctx, new ConsoleHub());
            var simulation = simContext.InitializeSimulation(simConfig).Result;
            
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
                                                                    , simContext.ContractCollector });
                await sim;
            }
            
            Assert.True(simWasReady);
        }
    }
}
