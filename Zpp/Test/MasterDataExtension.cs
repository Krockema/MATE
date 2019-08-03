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
            int businessPartnerId =
                productionDomainContext.BusinessPartners.Single(x => x.Name == "Tischverkäufer").Id;
            var order = new T_CustomerOrder
            {
                BusinessPartnerId = businessPartnerId,
                CreationTime = 10,
                Name = "BeispielOrder 1",
                DueTime = 30
            };
            productionDomainContext.Add(order);
            productionDomainContext.SaveChanges();
            var orderPart = new T_CustomerOrderPart
            {
                ArticleId = productionDomainContext.Articles.Single(x => x.Name == "Tisch").Id,
                Quantity = quantity,
                CustomerOrderId = order.Id
            };
            productionDomainContext.Add(orderPart);
            productionDomainContext.SaveChanges();
        }
    }
}