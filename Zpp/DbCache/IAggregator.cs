using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.MrpRun.MachineManagement;

namespace Zpp.DbCache
{
    /**
     * A layer over masterData/transactionData that provides aggregations of entities from masterData/transactionData
     */
    public interface IAggregator
    {
        ProductionOrderBoms GetProductionOrderBomsOfProductionOrder(ProductionOrder productionOrder);

        List<Machine> GetMachinesOfProductionOrderOperation(ProductionOrderOperation productionOrderOperation);

        List<ProductionOrderOperation> GetProductionOrderOperationsOfMachine(Machine machine);
        
        List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(ProductionOrder productionOrder);
        
        List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(Id productionOrderId);

        Demands GetDemandsOfProvider(Provider provider);

        ProductionOrderBom GetAnyProductionOrderBomByProductionOrderOperation(ProductionOrderOperation productionOrderOperation);
    }
}