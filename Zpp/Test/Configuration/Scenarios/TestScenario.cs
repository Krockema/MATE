using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DbCache;

namespace Zpp.Test.Configuration.Scenarios
{
    public abstract class TestScenario
    {
        protected readonly IDbMasterDataCache DbMasterDataCache;
        
        public TestScenario(IDbMasterDataCache dbMasterDataCache)
        {
            DbMasterDataCache = dbMasterDataCache;
        }
        
        public abstract void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext);
    }
}