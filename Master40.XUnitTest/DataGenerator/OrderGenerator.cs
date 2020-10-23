using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Master40.DB;
using Master40.SimulationCore.Environment.Options;
using Xunit;
using Xunit.Abstractions;

namespace Master40.XUnitTest.DataGenerator
{
    public class OrderGenerator
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public OrderGenerator(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        [Fact]
        public void CreateOrders()
        {
            long currentTime = 0;
            int orderCount = 100;

            var dataBase = Dbms.GetNewMasterDataBase(dbName: "Master40");

            var productIds = dataBase.DbContext.Articles.Include(x => x.ArticleType).Where(x => x.ArticleType.Name.Equals("Product")).Select(x => x.Id).ToList();
            var simConfig = new SimulationCore.Environment.Configuration();
            simConfig.AddOption(new Seed(169));
            simConfig.AddOption(new OrderArrivalRate(0.015));
            simConfig.AddOption(new MinDeliveryTime(1920));
            simConfig.AddOption(new MaxDeliveryTime(2880));
            var _orderGenerator = new SimulationCore.Helper.DistributionProvider.OrderGenerator(simConfig, dataBase.DbContext, productIds);


            for (int i = 0; i < orderCount; i++)
            {
                var order = _orderGenerator.GetNewRandomOrder(time: currentTime);
                currentTime = order.CreationTime;
                if (order.CreationTime > 10080)
                    break;
                dataBase.DbContext.CustomerOrders.Add(order);
              
            }

            dataBase.DbContext.SaveChanges();
        }
    }
}
