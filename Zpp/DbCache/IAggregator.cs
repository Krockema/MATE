using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.Mrp.MachineManagement;
using Zpp.WrappersForPrimitives;

namespace Zpp.DbCache
{
    /**
     * A layer over masterData/transactionData that provides aggregations of entities from masterData/transactionData
     */
    public interface IAggregator
    {
        ProductionOrderBoms GetProductionOrderBomsOfProductionOrder(ProductionOrder productionOrder);

        List<Resource> GetResourcesByResourceSkillId(Id resourceSkillId);

        List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(ProductionOrder productionOrder);

        List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(Id productionOrderId);

        ProductionOrderBom GetAnyProductionOrderBomByProductionOrderOperation(ProductionOrderOperation productionOrderOperation);

        ProductionOrderBoms GetAllProductionOrderBomsBy(
            ProductionOrderOperation productionOrderOperation);

        Providers GetAllChildProvidersOf(Demand demand);

        Providers GetAllParentProvidersOf(Demand demand);

        Demands GetAllParentDemandsOf(Provider provider);

        Demands GetAllChildDemandsOf(Provider provider);


        List<Provider> GetProvidersForInterval(DueTime from, DueTime to);

    }
}