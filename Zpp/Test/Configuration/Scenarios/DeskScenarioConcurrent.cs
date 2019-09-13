using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Zpp.Test.Configuration.Scenarios
{
    public class DeskScenarioConcurrent : TestScenario
    {
        public DeskScenarioConcurrent(IDbMasterDataCache dbMasterDataCache) : base(dbMasterDataCache)
        {
        }

        public override void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            CreateCustomerOrdersWithDesks(productionDomainContext,
                (int)quantity.GetValue());

            
        }
        
        private void CreateCustomerOrdersWithDesks(ProductionDomainContext productionDomainContext, int quantity)
        {
            
            for (int i = 0; i < quantity; i++)
            {

                CustomerOrderPart customerOrderPart = EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(DbMasterDataCache, 1,
                    new DueTime(50));
                productionDomainContext.Add(customerOrderPart.GetValue());
            }

            productionDomainContext.SaveChanges();
        }
    }
}