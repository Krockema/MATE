using System.Collections.Generic;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Zpp.MrpRun.MachineManagement
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