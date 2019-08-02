using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DemandDomain;

namespace Zpp
{
    /// <summary>
    /// A wrapper around the ProductionDomainContext (e.g. to find DB calls), that should be DB Cache in future.
    /// This class is only allowed to do transactions on the database.
    /// Convention: Prefix DatabaseBeingAffected
    ///
    /// MasterData includes T_CustomerOrders and T_CustomerOrderParts, since they will not be changed by MRP-Run
    /// </summary>
    public interface IDbMasterDataCache
    {
        M_Article M_ArticleGetById(Id id);
        
        List<M_Article> M_ArticleGetAll();

        M_ArticleBom M_ArticleBomGetById(Id id);

        List<M_BusinessPartner> M_BusinessPartnerGetAll();
        
        M_BusinessPartner M_BusinessPartnerGetById(Id id);

        M_ArticleToBusinessPartner M_ArticleToBusinessPartnerGetById(Id id);
        
        BusinessPartners M_ArticleToBusinessPartnerGetAllBusinessPartnersByArticleId(Id articleId);
        
        List<M_ArticleToBusinessPartner> M_ArticleToBusinessPartnerGetAllByArticleId(Id articleId);

        M_ArticleType M_ArticleTypeGetById(Id id);

        M_Machine M_MachineGetById(Id id);

        M_MachineGroup M_MachineGroupGetById(Id id);

        M_MachineTool M_MachineToolGetById(Id id);

        M_Operation M_OperationGetById(Id id);

        M_Stock M_StockGetById(Id id);
        
        List<M_Stock> M_StockGetAll();
        
        /**
         * returns the stock for given article if such exist, else null
         */
        M_Stock M_StockGetByArticleId(Id articleId);

        M_Unit M_UnitGetById(Id id);

        T_CustomerOrder T_CustomerOrderGetById(Id id);

        T_CustomerOrderPart T_CustomerOrderPartGetById(Id id);

        List<T_CustomerOrder> T_CustomerOrderGetAll();

        Demands T_CustomerOrderPartGetAll();

        List<M_ArticleBom> M_ArticleBomGetRootArticles();

    }
}