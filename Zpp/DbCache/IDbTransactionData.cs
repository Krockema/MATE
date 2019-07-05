using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp;
using Zpp.DemandToProviderDomain;

namespace Zpp
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

        void PersistDbCache(IDemandToProviders demandToProviders);

        void DemandsAdd(Demand demand);
        
        void DemandsAddAll(IDemands demands);
        
        
        IProviders ProvidersGetAll();

        void ProvidersAdd(Provider provider);
        
        void ProvidersAddAll(IProviders providers);
        
        ProductionOrderBoms ProductionOrderBomGetAll();
        
        StockExchangeProviders StockExchangeGetAll();
        
        PurchaseOrderParts PurchaseOrderPartGetAll();

        ProductionOrders ProductionOrderGetAll();

        Demands DemandsGetAll();

        void DemandToProviderAddAll(IDemandToProviders demandToProviders);
        
        IDemandToProviderTable DemandToProviderGetAll();

        Demand DemandsGetById(Id id);
        
        Provider ProvidersGetById(Id id);
        
    }
}