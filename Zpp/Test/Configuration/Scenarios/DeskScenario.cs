using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.Test.Configurations.Scenarios
{
    public class DeskScenario : TestScenario
    {
        public void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            MasterDataExtension.CreateCustomerOrdersWithDesks(productionDomainContext,
                (int)quantity.GetValue());

            
        }
    }
}