using System.Collections.Generic;
using System.Linq;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Zpp.Mrp.MachineManagement
{
    public class PriorityRule : IPriorityRule
    {
        public ProductionOrderOperation GetHighestPriorityOperation(DueTime now,
            List<ProductionOrderOperation> productionOrderOperations,
            IDbTransactionData dbTransactionData)
        {
            if (productionOrderOperations.Any()==false)
            {
                return null;
            }
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                ProductionOrder productionOrder =
                    dbTransactionData.ProductionOrderGetById(productionOrderOperation
                        .GetProductionOrderId());
                // TODO: This is different from specification
                DueTime minStartNextOfParentProvider =
                    productionOrder.GetDueTime(dbTransactionData);
                
                Priority priority = GetPriorityOfProductionOrderOperation(now,
                    productionOrderOperation, dbTransactionData, minStartNextOfParentProvider);
                productionOrderOperation.SetPriority(priority);
            }

            return productionOrderOperations.OrderBy(x => x.GetPriority().GetValue()).ToList()[0];
        }

        public Priority GetPriorityOfProductionOrderOperation(DueTime now,
            ProductionOrderOperation givenProductionOrderOperation,
            IDbTransactionData dbTransactionData, DueTime minStartNextOfParentProvider)
        {
            Dictionary<HierarchyNumber, DueTime> alreadySummedHierarchyNumbers =
                new Dictionary<HierarchyNumber, DueTime>();
            DueTime sumDurationsOfOperations = DueTime.Null();
            List<ProductionOrderOperation> productionOrderOperations = dbTransactionData
                .GetAggregator()
                .GetProductionOrderOperationsOfProductionOrder(givenProductionOrderOperation
                    .GetProductionOrderId());
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                // only later operations, which have a smaller hierarchyNumber, have to be considered
                if (productionOrderOperation.GetHierarchyNumber()
                    .IsSmallerThan(givenProductionOrderOperation.GetHierarchyNumber()))
                {
                    continue;
                }

                if (alreadySummedHierarchyNumbers.ContainsKey(productionOrderOperation
                    .GetHierarchyNumber()))
                {
                    DueTime alreadySummedHierarchyNumber =
                        alreadySummedHierarchyNumbers[
                            productionOrderOperation.GetHierarchyNumber()];
                    if (productionOrderOperation.GetDuration()
                        .IsGreaterThan(alreadySummedHierarchyNumber))
                    {
                        sumDurationsOfOperations.IncrementBy(productionOrderOperation.GetDuration()
                            .Minus(alreadySummedHierarchyNumber));
                    }
                }
                else
                {
                    alreadySummedHierarchyNumbers.Add(productionOrderOperation.GetHierarchyNumber(),
                        productionOrderOperation.GetDuration());
                }
            }

            return new Priority(minStartNextOfParentProvider.Minus(now)
                .Minus(sumDurationsOfOperations).GetValue());
        }
    }
}