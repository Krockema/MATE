using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Agents;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Enums;
using Master40.DB.Migrations;
using Master40.Simulation.Simulation;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace Master40.XUnitTest.DBContext
{
    public class ContextTest
    {
        ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
            .Options);

        CopyContext _copyContext = new CopyContext(new DbContextOptionsBuilder<MasterDBContext>()
        .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        MasterDBContext _masterDBContext = new MasterDBContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        ProductionDomainContext _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);

        public ContextTest()
        {
            _ctx.Database.EnsureDeleted();
            MasterDBInitializerLarge.DbInitialize(_ctx);
        }

        /// <summary>
        /// to Cleanup ctx for every Test // uncomment this. 
        /// </summary>
        /*
        public void Dispose()
        {
            _ctx.Dispose();
        }
        */



        [Fact]
        public void OrderContextTest()
        {
            _ctx.Orders.Add(new Order {Name = "Order1"});
            _ctx.SaveChanges();

            Assert.Equal(2, _ctx.Orders.Count());
        }

        [Fact]
        public async Task MrpTestAsync()
        {
            var scheduling = new Scheduling(_ctx);
            var capacityScheduling = new CapacityScheduling(_ctx);
            var msgHub = new Moc.MessageHub();
            var rebuildNets = new RebuildNets(_ctx);
            var mrpContext = new ProcessMrp(_ctx, scheduling, capacityScheduling, msgHub, rebuildNets);

            var mrpTest = new MrpTest();
            await mrpTest.CreateAndProcessOrderDemandAll(mrpContext);
            Assert.Equal(true, (_ctx.ProductionOrderWorkSchedules.Any()));

        }

        //public DemandToProvider getRequester
        [Fact]
        public async Task AgentSimulationTestAsync()
        {
            var sim = new AgentSimulation(_ctx, new Moc.MessageHub());
            await sim.RunSim();

            Assert.Equal(true, true);
        }


        [Fact]
        public async Task MrpTestForwardAsync()
        {
            var scheduling = new Scheduling(_productionDomainContext);
            var capacityScheduling = new CapacityScheduling(_productionDomainContext);
            var msgHub = new Moc.MessageHub();
            var rebuildNets = new RebuildNets(_productionDomainContext);
            var mrpContext = new ProcessMrp(_productionDomainContext, scheduling, capacityScheduling, msgHub, rebuildNets);
            var simulation = new Simulator(_productionDomainContext,msgHub);
            var mrpTest = new MrpTest();
            await mrpTest.CreateAndProcessOrderDemandAll(mrpContext);
            //await simulation.Simulate();

            Assert.Equal(true, _productionDomainContext.ProductionOrderWorkSchedules.Any());
        }

        // Load Database from SimulationJason
        [Fact]
        public async Task LoadContextAsync()
        {
            var simState = _ctx.SaveSimulationState();


            var stringSimState =
                JsonConvert.SerializeObject(simState, Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }
                );

            var deserialized = JsonConvert.DeserializeObject<SimulationDbState>(stringSimState);

            _masterDBContext.Database.EnsureDeleted();
            _masterDBContext.Database.EnsureCreated();
            _copyContext.LoadContextFromSimulation(deserialized);


            Assert.Equal(true, (_ctx.Articles.Any()));

        }


        // HardDatabase To InMemory
        [Fact]
        public async Task CopyContext()
        {
            _productionDomainContext.Database.EnsureCreated();
            MasterDBInitializerLarge.DbInitialize(_productionDomainContext);
            _ctx.Database.EnsureCreated();
            _productionDomainContext.CopyAllTables(_ctx);
            
            Assert.Equal(true, (_ctx.Articles.Any()));
        }

        // Json to InMemory
        [Fact]
        public async Task CopyJsonToInMemmory()
        {

            var json = _ctx.SaveSimulationState();

            _ctx.Database.EnsureDeleted();
            _ctx.Database.EnsureCreated();

            _ctx.LoadInMemoryDB(json);

            Assert.Equal(true, (_ctx.Articles.Any()));

        }

    }
}

