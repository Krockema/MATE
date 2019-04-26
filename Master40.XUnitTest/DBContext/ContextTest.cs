using System;
using System.Collections.Generic;
using System.Linq;
using EfCore.InMemoryHelpers;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Microsoft.EntityFrameworkCore;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;
using Master40.SimulationCore.Helper;
using Xunit;

namespace Master40.XUnitTest.DBContext
{
    public class ContextTest
    {
         ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
             .UseInMemoryDatabase(databaseName: "InMemoryDB")
             .Options);


        // InMemoryContext _inMemmoryContext = new InMemoryContext(new DbContextOptionsBuilder<MasterDBContext>()
        //     .UseInMemoryDatabase(databaseName: "InMemoryDB")
        //     .Options);

        InMemoryContext _inMemmoryContext = InMemoryContextBuilder.Build<InMemoryContext>(new DbContextOptionsBuilder<MasterDBContext>());



        MasterDBContext _masterDBContext = new MasterDBContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options);
        
        // ProductionDomainContext _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
        //     .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true")
        //     .Options);

        public ContextTest()
        {
            _ctx.Database.EnsureDeleted();
            _ctx.Database.EnsureCreated();
            MasterDBInitializerLarge.DbInitialize(_ctx);
            //MasterDBInitializerSmall.DbInitialize(_ctx);
            // _productionDomainContext.Database.EnsureDeleted();
            // _productionDomainContext.Database.EnsureCreated();
            // MasterDBInitializerLarge.DbInitialize(_productionDomainContext);

        }

        //[Fact]
        public void DBCreationTest()
        {
            _masterDBContext.Database.EnsureDeleted();
            _masterDBContext.Database.EnsureCreated();
            MasterDBInitializerSmall.DbInitialize(_masterDBContext);
            //MasterDBInitializerSmall.DbInitialize(_ctx);
            // _productionDomainContext.Database.EnsureDeleted();
            // _productionDomainContext.Database.EnsureCreated();
            // MasterDBInitializerLarge.DbInitialize(_productionDomainContext);

        }



        public static SimulationConfiguration TestConfiguration()
        {
            return new SimulationConfiguration
            {
                Name = "Test config",
                Lotsize = 1,
                MaxCalculationTime = 480, // test  // 10080, // 7 days
                OrderQuantity = 550,
                Seed = 1338,
                ConsecutiveRuns = 1,
                OrderRate = 0.025, //0.25
                Time = 600,
                RecalculationTime = 1440,
                SimulationEndTime = 20160,
                DecentralRuns = 0,
                CentralRuns = 0,
                DynamicKpiTimeSpan = 480,
                SettlingStart = 0,
                WorkTimeDeviation = 0.2

            };
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
        /*
        [Fact]
        public async Task MrpTestAsync()
        {
            var imc = InMemoryContext.CreateInMemoryContext();
            InMemoryContext.LoadData(_productionDomainContext, imc);
            var msgHub = new Moc.MessageHub();
            var simulation = new Simulator(imc, msgHub);
            await simulation.Simulate(1);
            Assert.True(_productionDomainContext.Kpis.Any());
        }
        

        */

        [Fact]
        public void TestContext()
        {
            OrderGenerator.GenerateOrdersSyncron(_ctx, ContextTest.TestConfiguration(), 1, true); // .RunSynchronously();
            Console.WriteLine("Orders created.");
        }

        [Fact]
        public void TableTest()
        {
            var art = new M_Article {Name = "New"};
            _ctx.Add(art);




            // var dbsets = _ctx.GetType().GetProperties().Where(x => x.DeclaringType == typeof(MasterDBContext)).ToList();
            // foreach (var dbset in dbsets)
            // {
            //     if (dbset.Name != "InMemory")
            //     {
            //         dynamic setValue = _ctx.GetType().GetMember(dbset.Name).GetValue(0);
            //     // dynamic propValue = dbset.GetValue(_ctx);
            //     // Type type = Type.GetType("Master40.DB.DataModel.M_Article", "");
            //     // 
            //     // var listType = typeof(List<>);
            //     // var constructedListType = listType.MakeGenericType(Type.GetType(dbset.Name));
            //     // var instance = Activator.CreateInstance(constructedListType);
            //     // 
            //     // var tolist = typeof(Enumerable).GetMethod("ToList");
            //     // 
            //     // 
            //     // tolist = tolist.MakeGenericMethod(type);
            //     // var ret = tolist.Invoke(type, propValue);
            //         return;
            //     }
            // }

        }

    }
}

