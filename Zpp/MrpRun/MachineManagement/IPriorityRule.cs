using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp.MachineDomain
{
    public interface IPriorityRule
    {
        Priority GetPriorityOfProductionOrderOperation(DueTime now,
            ProductionOrderOperation givenProductionOrderOperation,
            IDbTransactionData dbTransactionData, DueTime minStartNextOfParentProvider);

        ProductionOrderOperation GetHighestPriorityOperation(DueTime now,
            List<ProductionOrderOperation> productionOrderOperations,
            IDbTransactionData dbTransactionData);
    }
}