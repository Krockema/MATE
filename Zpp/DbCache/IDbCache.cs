using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    /// <summary>
    /// A wrapper around the ProductionDomainContext (e.g. to find DB calls), that should be DB Cache in future.
    /// This class is only allowed to do transactions on the database.
    /// Convention: Prefix DatabaseBeingAffected
    /// </summary>
    public interface IDbCache
    {
        void DemandToProvidersRemoveAll();

        void PersistDbCache();

        void PurchaseOrderAdd(PurchaseOrder purchaseOrder);

        List<M_BusinessPartner> M_BusinessPartnerGetAll();

        M_ArticleBom M_ArticleBomGetById(int id);
        
        M_Article M_ArticleGetById(int id);

        void DemandsAdd(Demand demand);
        
        void DemandsAddAll(Demands demands);
        
        
        Providers ProvidersGetAll();

        void ProvidersAdd(Provider provider);
        
        void ProvidersAddAll(Providers providers);
        
        
        CustomerOrderParts CustomerOrderPartGetAll();

        ProductionOrderBoms ProductionOrderBomGetAll();
        
        StockExchangeProviders StockExchangeGetAll();
        
        PurchaseOrderParts PurchaseOrderPartGetAll();

        ProductionOrders ProductionOrderGetAll();

        Demands DemandsGetAll();

    }
}