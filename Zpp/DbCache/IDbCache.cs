using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp;

namespace Zpp
{
    /// <summary>
    /// A wrapper around the ProductionDomainContext (e.g. to find DB calls), that should be DB Cache in future.
    /// This class is only allowed to do transactions on the database.
    /// Convention: Prefix DatabaseBeingAffected
    /// </summary>
    public interface IDbCache
    {
        M_Article M_ArticleGetById(Id id);
        
        M_ArticleBom M_ArticleBomGetById(Id id);
        
        List<M_BusinessPartner> M_BusinessPartnerGetAll();
        
        void DemandToProvidersRemoveAll();

        void PersistDbCache();

        void PurchaseOrderAdd(PurchaseOrder purchaseOrder);

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