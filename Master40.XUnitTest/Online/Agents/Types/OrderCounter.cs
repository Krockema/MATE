using System.Data.Entity;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper.DistributionProvider;
using Xunit;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class OrderCounter
    {
        SimulationCore.Agents.SupervisorAgent.Types.OrderCounter _orderCounter = new SimulationCore.Agents.SupervisorAgent.Types.OrderCounter(maxQuantity: 2);
        
        [Fact]
        public void AddOrder()
        {
            bool addNewOrderSuccessful = _orderCounter.TryAddOne();
            Assert.True(condition: addNewOrderSuccessful);
        }


        [Fact]
        public void AddOrderOverMax()
        {
            //add 2
            _orderCounter.TryAddOne();
            bool addNewOrderSuccessful = _orderCounter.TryAddOne();
            Assert.True(condition: addNewOrderSuccessful);
            //add more than 2
            bool addNewOrderOverMaxExeeded = _orderCounter.TryAddOne();
            Assert.False(condition: addNewOrderOverMaxExeeded);
        }

        [Fact]
        public void CreateOrders()
        {
            long currentTime = 0;
            int orderCount = 1000;

            var dataBase = Dbms.GetNewMasterDataBase(dbName: "Master40");

            var productIds = dataBase.DbContext.Articles.Include(x => x.ArticleType).Where(x => x.ArticleType.Name.Equals("Product")).Select(x => x.Id).ToList();
            var simConfig = new SimulationCore.Environment.Configuration();
            simConfig.AddOption(new Seed(169));
            simConfig.AddOption(new OrderArrivalRate(0.015));
            simConfig.AddOption(new MinDeliveryTime(1920));
            simConfig.AddOption(new MaxDeliveryTime(2880));
            var _orderGenerator = new OrderGenerator(simConfig,dataBase.DbContext,productIds);


            for (int i = 0; i < orderCount; i++)
            {
                var order = _orderGenerator.GetNewRandomOrder(time: currentTime);
                currentTime = order.CreationTime;

                dataBase.DbContext.CustomerOrders.Add(order);

                if (order.CreationTime > 10080)
                    break;

            }

            dataBase.DbContext.SaveChanges();
        }

    }
}
