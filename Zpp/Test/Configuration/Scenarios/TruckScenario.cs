using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;

namespace Zpp.Test.Configuration.Scenarios
{
    public class TruckScenario : TestScenario
    {
        public void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            OrderGenerator.GenerateOrdersSyncron(productionDomainContext,
                ContextTest.TestConfiguration(), 1, true,
                (int)quantity.GetValue());
        }
    }
}