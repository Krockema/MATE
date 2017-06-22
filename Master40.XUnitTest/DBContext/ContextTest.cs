using Master40.DB.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Master40.DB.Data.Context;
using Xunit;

namespace Master40.XUnitTest
{
    public class ContextTest
    {

        [Fact]
        public void OrderContextTest()
        {

            var options = new DbContextOptionsBuilder<OrderDomainContext>()
                    .UseInMemoryDatabase(databaseName: "InMemoryDB")
                    .Options;

            // Run the test against one instance of the context
            using (var context = new OrderDomainContext(options))
            {
                context.Orders.Add(new DB.Models.Order { Name = "Order1" });
                context.SaveChanges();
            }

            // Use a separate instance of the context to verify correct data was saved to database
            using (var context = new OrderDomainContext(options))
            {
                Assert.Equal(1, context.Orders.Count());
                Assert.Equal("Order1", context.Orders.First().Name);
            }
        }
    }
}
