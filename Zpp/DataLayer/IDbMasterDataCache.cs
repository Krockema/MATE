using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler;

namespace Zpp.DataLayer
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

        M_Article M_ArticleGetByName(string name);

        List<M_Article> M_ArticleGetAll();

        List<M_Article> M_ArticleGetArticlesToBuy();

        M_ArticleBom M_ArticleBomGetById(Id id);

        M_ArticleBom M_ArticleBomGetByArticleChildId(Id id);

        List<M_BusinessPartner> M_BusinessPartnerGetAll();

        M_BusinessPartner M_BusinessPartnerGetById(Id id);

        M_ArticleToBusinessPartner M_ArticleToBusinessPartnerGetById(Id id);

        BusinessPartners M_ArticleToBusinessPartnerGetAllBusinessPartnersByArticleId(Id articleId);

        List<M_ArticleToBusinessPartner> M_ArticleToBusinessPartnerGetAllByArticleId(Id articleId);
        List<Resource> ResourcesGetAllBySkillId(Id id);
        M_ArticleType M_ArticleTypeGetById(Id id);

        Resource M_ResourceGetById(Id id);

        List<M_ResourceSetup> M_ResourceSetupGetAll();

        List<Resource> ResourceGetAll();

        M_ResourceSkill M_ResourceSkillById(Id id);

        List<M_ResourceSkill> M_ResourceSkillGetAll();

        M_ResourceTool M_ResourceToolGetById(Id id);

        M_Operation M_OperationGetById(Id id);

        M_Stock M_StockGetById(Id id);

        List<M_Stock> M_StockGetAll();

        void M_StockSetAll(List<M_Stock> stocks);

        /**
         * returns the stock for given article if such exist, else null
         */
        M_Stock M_StockGetByArticleId(Id articleId);

        M_Unit M_UnitGetById(Id id);
    }
}