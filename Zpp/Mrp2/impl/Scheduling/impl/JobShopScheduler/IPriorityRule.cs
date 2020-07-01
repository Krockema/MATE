using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;

namespace Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler
{
    public interface IPriorityRule
    {

        ProductionOrderOperation GetHighestPriorityOperation(DueTime now,
            List<ProductionOrderOperation> productionOrderOperations);
    }
}