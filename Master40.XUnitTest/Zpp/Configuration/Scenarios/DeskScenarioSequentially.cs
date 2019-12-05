using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Master40.XUnitTest.Zpp.Configuration.Scenarios
{
    /**
     * This scenario enforces:
     * - forward scheduling (since first dueTime for COP is too short)
     * - a time longer in the future than the production time takes
     */
    public class DeskScenarioSequentially : TestScenario
    {
        public DeskScenarioSequentially(IDbMasterDataCache dbMasterDataCache) : base(dbMasterDataCache)
        {
        }

        public override void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            CreateCustomerOrdersWithDesks(productionDomainContext,
                (int)quantity.GetValue());

            
        }
        
        private void CreateCustomerOrdersWithDesks(ProductionDomainContext productionDomainContext, int quantity)
        {
            
            for (int i = 0, currentDueTime = 50; i < quantity; i++, currentDueTime+=50)
            {

                CustomerOrderPart customerOrderPart = EntityFactory.CreateCustomerOrderPartRandomArticleToBuy(DbMasterDataCache, 1,
                    new DueTime(currentDueTime));
                productionDomainContext.Add<T_CustomerOrderPart>(customerOrderPart.GetValue());
            }

            productionDomainContext.SaveChanges();
        }
    }
}