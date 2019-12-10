using System.Linq;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationMrp.impl.CustomerOrder.impl;
using Master40.XUnitTest.Zpp.Integration_Tests;
using Xunit;
using Zpp;
using Zpp.DataLayer;
using Zpp.Utils;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestOrderGenerator : AbstractTest
    {
        public TestOrderGenerator() : base()
        {
        }

        [Fact]
        public void TestGeneratedQuantity()
        {
            ICentralPlanningConfiguration testConfiguration =
                ZppConfiguration.CacheManager.GetTestConfiguration();
            int cycles = testConfiguration.SimulationMaximumDuration /
                         testConfiguration.SimulationInterval;
            int totalCustomerOrdersToCreate = 500;
            int customerOrdersToCreate = totalCustomerOrdersToCreate / (cycles + 1); // round up


            CustomerOrderCreator og = new CustomerOrderCreator(null);
            og.CreateCustomerOrders(new SimulationInterval(0, 20160),
                new Quantity(totalCustomerOrdersToCreate));
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            var orders = dbTransactionData.CustomerOrderGetAll();
            Assert.InRange(orders.Count(), customerOrdersToCreate, customerOrdersToCreate+10);
        }
    }
}