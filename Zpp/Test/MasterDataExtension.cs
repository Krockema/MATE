using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Zpp.Test
{
    public class MasterDataExtension
    {
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
                    CreationTime = random.Next(5, 50),
                    Name = "ExampleOrder 1",
                    DueTime = random.Next(1, 100)
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