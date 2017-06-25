using Master40.DB.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Xunit;
using Master40.BusinessLogic.MRP;
using Master40.DB.Data.Initializer;
using Master40.XUnitTest.DBContext;

namespace Master40.XUnitTest
{
    public class ContextTest
    {
        readonly MasterDBContext _ctx = new MasterDBContext(new DbContextOptionsBuilder<MasterDBContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDB")
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
            _ctx.Orders.Add(new DB.Models.Order { Name = "Order1" });
            _ctx.SaveChanges();

            Assert.Equal(2, _ctx.Orders.Count());
        }

        [Fact]
        public async Task MrpTestAsync()
        {
            var scheduling = new Scheduling(_ctx);
            var capacityScheduling = new CapacityScheduling(_ctx);
            var mrpContext = new ProcessMrp(_ctx, scheduling, capacityScheduling);

            var mrpTest = new MrpTest();
            await mrpTest.CreateAndProcessOrderDemandAll(mrpContext);

            Assert.Equal(true, (_ctx.ProductionOrderWorkSchedule.Any()));

        }


    }
}
