using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Master40.XUnitTest.DBContext
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
            _ctx.Orders.Add(new Order { Name = "Order1" });
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
