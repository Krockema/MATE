using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Environment.Options;
using Microsoft.EntityFrameworkCore;
using Zpp.DbCache;

namespace Master40.XUnitTest.Zpp.Configuration.Scenarios
{
    public class TruckScenario : TestScenario
    {
        public TruckScenario(IDbMasterDataCache dbMasterDataCache) : base(dbMasterDataCache)
        {
        }

        public override void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            var config = Master40.SimulationCore.Environment.Configuration.Create(args: new object[]
            {
                new Seed(value: 2),
                new SimulationNumber(value: 1),
                new OrderArrivalRate(value: 0.025),
                new MinDeliveryTime(value: 300), 
                new MaxDeliveryTime(value: 600), 
            });
            var productIds = productionDomainContext.Articles.Include(x => x.ArticleType)
                .Where(x => x.ArticleType.Name == "Product").Select(x => x.Id)
                .ToList();
            var orderGenerator = new OrderGenerator(config, productionDomainContext, productIds);
            productionDomainContext.CustomerOrders.Add(orderGenerator.GetNewRandomOrder(0));
            productionDomainContext.SaveChanges();
        }
    }
}