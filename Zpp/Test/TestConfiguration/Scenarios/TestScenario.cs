using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.Test.Configurations.Scenarios
{
    public interface TestScenario
    {
        void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext);
    }
}