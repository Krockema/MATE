using System.Collections.Generic;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler;

namespace Zpp.DataLayer
{
    /**
     * A layer over masterData/transactionData that provides aggregations of entities from masterData/transactionData
     */
    public interface IAggregator
    {
        ProductionOrderBoms
            GetProductionOrderBomsOfProductionOrder(ProductionOrder productionOrder);

        List<Resource> GetResourcesByResourceCapabilityId(Id resourceCapabilityId);

        List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(
            ProductionOrder productionOrder);

        List<ProductionOrderOperation> GetProductionOrderOperationsOfProductionOrder(
            Id productionOrderId);

        ProductionOrderBom GetAnyProductionOrderBomByProductionOrderOperation(
            ProductionOrderOperation productionOrderOperation);

        Providers GetAllChildProvidersOf(Demand demand);
        
        Providers GetAllChildStockExchangeProvidersOf(ProductionOrderOperation operation);

        Providers GetAllParentProvidersOf(Demand demand);

        Demands GetAllParentDemandsOf(Provider provider);

        Demands GetAllChildDemandsOf(Provider provider);


        List<Provider> GetProvidersForInterval(DueTime from, DueTime to);
        
        /**
         * Traverse down till including StockExchangeDemands and calculate max endTime of the children
         */
        DueTime GetEarliestPossibleStartTimeOf(ProductionOrderOperation productionOrderOperation);

        Demands GetUnsatisfiedCustomerOrderParts();

        /**
         * return DemandsOrProviders' EndTime within given interval AND EndTime BEFORE the startOfInterval
         */
        DemandOrProviders GetDemandsOrProvidersWhereEndTimeIsWithinIntervalOrBefore(
            SimulationInterval simulationInterval, DemandOrProviders demandOrProviders);

        DemandOrProviders GetDemandsOrProvidersWhereStartTimeIsWithinInterval(
            SimulationInterval simulationInterval, DemandOrProviders demandOrProviders);

        /**
         * Arrow equals DemandToProvider and ProviderToDemand
         */
        List<ILinkDemandAndProvider> GetArrowsToAndFrom(Provider provider);
        
        List<ILinkDemandAndProvider> GetArrowsTo(Provider provider);
        
        List<ILinkDemandAndProvider> GetArrowsTo(IDemandOrProvider provider);
        
        List<ILinkDemandAndProvider> GetArrowsFrom(IDemandOrProvider demandOrProvider);
        
        List<ILinkDemandAndProvider> GetArrowsFrom(Provider provider);
        
        List<ILinkDemandAndProvider> GetArrowsTo(Demand demand);
        
        List<ILinkDemandAndProvider> GetArrowsFrom(Demand demand);
        
        List<ILinkDemandAndProvider> GetArrowsTo(Providers providers);
        
        List<ILinkDemandAndProvider> GetArrowsFrom(Providers providers);
        
        List<ILinkDemandAndProvider> GetArrowsTo(Demands demands);
        
        List<ILinkDemandAndProvider> GetArrowsFrom(Demands demands);

        /**
         * Arrow equals DemandToProvider and ProviderToDemand
         */
        List<ILinkDemandAndProvider> GetArrowsToAndFrom(Demand demand);
        
        /**
         * Arrow equals DemandToProvider and ProviderToDemand
         */
        List<ILinkDemandAndProvider> GetArrowsToAndFrom(IDemandOrProvider demandOrProvider);

        List<ProductionOrderOperation> GetAllOperationsOnResource(M_Resource resource);

        Demands GetProductionOrderBomsBy(ProductionOrderOperation operation);
    }
}