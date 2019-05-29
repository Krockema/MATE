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
            var simContext = new AgentSimulation(true, _ctx, _ctxResult, new ConsoleHub());
            var simConfig = ContextTest.TestConfiguration();
            // simConfig.OrderQuantity = 0;
            var simModelConfig = new SimulationConfig(false, 480);
            var simulation = await simContext.InitializeSimulation(simConfig, simModelConfig);

            // simulation.ActorSystem.EventStream.Subscribe(testProbe, typeof(DirectoryAgent.Instruction.CreateMachineAgents));

            var simWasReady = false;
            if (simulation.IsReady())
            {
                // set for Assert 
                simWasReady = true;
                // Start simulation
                var sim = simulation.RunAsync();

                AgentSimulation.Continuation(simModelConfig.Inbox
                                            , simulation
                                            , new List<IActorRef> { simContext.StorageCollector
                                                                    , simContext.WorkCollector });
                await sim;
            }
            
            Assert.True(simWasReady);
        }
    }
}
