using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper.DistributionProvider;

namespace Master40.XUnitTest.Zpp.Configuration.Scenarios
{
    public abstract class TestScenario
    {
        public TestScenario()
        {
        }

        public static IOrderGenerator GetOrderGenerator(MinDeliveryTime minDeliveryTime,
            MaxDeliveryTime maxDelivery, OrderArrivalRate orderArrivalRate,
            IEnumerable<M_Article> articles, IEnumerable<M_BusinessPartner> businessPartners)
        {
            var config = Master40.SimulationCore.Environment.Configuration.Create(args: new object[]
            {
                new Seed(value: 1337),
                new SimulationNumber(value: 1),
                orderArrivalRate,
                maxDelivery,
                minDeliveryTime,
            });
            var productIds = articles.Where(x => x.ArticleType.Name == "Product").Select(x => x.Id).ToList();
            var orderGenerator = new OrderGeneratorPerformance(config, productIds, articles, businessPartners);
            return orderGenerator;
        }
    }
}