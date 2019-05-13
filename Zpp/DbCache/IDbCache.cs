using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp
{
    /// <summary>
    /// A wrapper around the ProductionDomainContext (e.g. to find DB calls), that should be DB Cache in future.
    /// This class is only allowed to do transactions on the database.
    /// Convention: Prefix DatabaseBeingAffected
    /// </summary>
    public interface IDbCache
    {
        void T_DemandToProvidersRemoveAll();

        void persistDbCache();

        void T_PurchaseOrderAdd(T_PurchaseOrder purchaseOrder);

        List<M_BusinessPartner> M_BusinessPartnerGetAll();

        M_ArticleBom M_ArticleBomGetById(int id);
        
        M_Article M_ArticleGetById(int id);

        List<T_Demand> T_DemandsGetAll();
        
        List<T_Provider> T_ProvidersGetAll();
        
        List<T_CustomerOrderPart> T_CustomerOrderPartGetAll();

        List<T_ProductionOrderBom> T_ProductionOrderBomGetAll();
        
        List<T_StockExchange> T_StockExchangeGetAll();
        
        List<T_PurchaseOrderPart> T_PurchaseOrderPartGetAll();

        List<T_ProductionOrder> T_ProductionOrderGetAll();

    }
}