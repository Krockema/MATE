using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp.Test.Configuration.Scenarios
{
    public class DeskScenario : TestScenario
    {
        public void CreateCustomerOrders(Quantity quantity, ProductionDomainContext productionDomainContext)
        {
            CreateCustomerOrdersWithDesks(productionDomainContext,
                (int)quantity.GetValue());

            
        }
        
        public static void CreateCustomerOrdersWithDesks(ProductionDomainContext productionDomainContext, int quantity)
        {
            Random random = new Random();
            
            int businessPartnerId =
                productionDomainContext.BusinessPartners.Single(x => x.Name == "Tischverkäufer").Id;
            for (int i = 0; i < quantity; i++)
            {
                var order = new T_CustomerOrder
                {
                    BusinessPartnerId = businessPartnerId,
                    CreationTime = random.Next(1, 5),
                    Name = "ExampleOrder 1",
                    // let the dueTime be constant to have parallel customerOrders
                    DueTime = 50
                };
                productionDomainContext.Add(order);

                var orderPart = new T_CustomerOrderPart
                {
                    ArticleId = productionDomainContext.Articles.Single(x => x.Name == "Tisch").Id,
                    Quantity = 1,
                    CustomerOrderId = order.Id
                };
                productionDomainContext.Add(orderPart);
            }

            productionDomainContext.SaveChanges();
        }
    }
}