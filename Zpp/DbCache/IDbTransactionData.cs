using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.Common.ProviderDomain;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Common.ProviderDomain.WrappersForCollections;
using Zpp.MrpRun.NodeManagement;
using Zpp.WrappersForCollections;

namespace Zpp.DbCache
{

    /**
     * NOTE: TransactionData does NOT include CustomerOrders or CustomerOrderParts !
     */
    public interface IDbTransactionData
    {
        // TODO: M_* methods should be removed
        M_Article M_ArticleGetById(Id id);
        
        M_ArticleBom M_ArticleBomGetById(Id id);
        
        List<M_BusinessPartner> M_BusinessPartnerGetAll();
        
        void DemandToProvidersRemoveAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="demandToProvidersMap">is used to generate T_Demand and T_Provider tables</param>
        void PersistDbCache();

        void SetProviderManager(IProviderManager providerManager);

        void DemandsAdd(Demand demand);
        
        void DemandsAddAll(IDemands demands);

        IProviders ProvidersGetAll();

        void ProvidersAdd(Provider provider);
        
        void ProvidersAddAll(IProviders providers);
        
        ProductionOrderBoms ProductionOrderBomGetAll();
        
        StockExchangeProviders StockExchangeProvidersGetAll();
        
        StockExchangeDemands StockExchangeDemandsGetAll();
        
        PurchaseOrderParts PurchaseOrderPartGetAll();
        
        T_PurchaseOrder PurchaseOrderGetById(Id id);
        
        List<T_PurchaseOrder> PurchaseOrderGetAll();

        ProductionOrders ProductionOrderGetAll();
        
        ProductionOrder ProductionOrderGetById(Id id);

        IDemands DemandsGetAll();

        IProviderToDemandTable ProviderToDemandGetAll();
        
        IDemandToProviderTable DemandToProviderGetAll();

        Demand DemandsGetById(Id id);
        
        Provider ProvidersGetById(Id id);

        IProviderManager GetProviderManager();

        T_ProductionOrderOperation ProductionOrderOperationGetById(Id id);
        
        List<ProductionOrderOperation> ProductionOrderOperationGetAll();

        IAggregator GetAggregator();
    }
}