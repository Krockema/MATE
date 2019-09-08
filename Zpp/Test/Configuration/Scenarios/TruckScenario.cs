using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Zpp.DbCache;

namespace Zpp.Test.Configuration.Scenarios
{
    public class TruckScenario : TestScenario
    {
        public TruckScenario(IDbMasterDataCache dbMasterDataCache) : base(dbMasterDataCache)
        {
        }

        public override void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            OrderGenerator.GenerateOrdersSyncron(productionDomainContext,
                ContextTest.TestConfiguration(), 1, true,
                (int)quantity.GetValue());
        }
    }
}