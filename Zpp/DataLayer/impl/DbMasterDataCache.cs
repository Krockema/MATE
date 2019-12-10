using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.DataLayer.impl
{
    /**
     * MasterData includes T_CustomerOrders and T_CustomerOrderParts, since they will not be changed by MRP-Run
     */
    public class DbMasterDataCache : IDbMasterDataCache
    {
        private readonly ProductionDomainContext _productionDomainContext;


        // cached tables
        // M_*

        private readonly IMasterDataTable<M_Article> _articles;
        private readonly IMasterDataTable<M_ArticleBom> _articleBoms;
        private readonly IMasterDataTable<M_ArticleToBusinessPartner> _articleToBusinessPartners;
        private readonly IMasterDataTable<M_ArticleType> _articleTypes;
        private readonly IMasterDataTable<M_BusinessPartner> _businessPartners;
        private readonly IMasterDataTable<M_Resource> _resources;
        private readonly IMasterDataTable<M_ResourceSkill> _resourceSkills;
        private readonly IMasterDataTable<M_ResourceSetup> _resourceSetups;
        private readonly IMasterDataTable<M_ResourceTool> _resourceTools;
        private readonly IMasterDataTable<M_Operation> _operations;
        private readonly IMasterDataTable<M_Stock> _stocks;
        private readonly IMasterDataTable<M_Unit> _units;

        public DbMasterDataCache(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;

            // cache tables
            _articles = new MasterDataTable<M_Article>(_productionDomainContext.Articles
                .Include(x => x.ArticleType).ToList());
            _articleBoms = new MasterDataTable<M_ArticleBom>(_productionDomainContext.ArticleBoms);
            _articleToBusinessPartners =
                new MasterDataTable<M_ArticleToBusinessPartner>(_productionDomainContext
                    .ArticleToBusinessPartners);
            _articleTypes =
                new MasterDataTable<M_ArticleType>(_productionDomainContext.ArticleTypes);
            _businessPartners =
                new MasterDataTable<M_BusinessPartner>(_productionDomainContext.BusinessPartners);
            _resources = new MasterDataTable<M_Resource>(_productionDomainContext.Resources);
            _resourceSkills =
                new MasterDataTable<M_ResourceSkill>(_productionDomainContext.ResourceSkills);
            _resourceTools =
                new MasterDataTable<M_ResourceTool>(_productionDomainContext.ResourceTools);
            _resourceSetups =
                new MasterDataTable<M_ResourceSetup>(_productionDomainContext.ResourceSetups);
            _operations = new MasterDataTable<M_Operation>(_productionDomainContext.Operations);
            _stocks = new MasterDataTable<M_Stock>(_productionDomainContext.Stocks);
            _units = new MasterDataTable<M_Unit>(_productionDomainContext.Units);
        }

        internal void Clone(ProductionDomainContext productionDomainContext)
        {
            DbTransactionData.InsertRange(_articles.GetAll(), productionDomainContext.Articles,
                productionDomainContext);
            DbTransactionData.InsertRange(_articleBoms.GetAll(),
                productionDomainContext.ArticleBoms, productionDomainContext);

            DbTransactionData.InsertRange(_articleToBusinessPartners.GetAll(),
                productionDomainContext.ArticleToBusinessPartners, productionDomainContext);

            DbTransactionData.InsertRange(_articleTypes.GetAll(),
                productionDomainContext.ArticleTypes, productionDomainContext);

            DbTransactionData.InsertRange(_businessPartners.GetAll(),
                productionDomainContext.BusinessPartners, productionDomainContext);

            DbTransactionData.InsertRange(_resources.GetAll(), productionDomainContext.Resources,
                productionDomainContext);

            DbTransactionData.InsertRange(_resourceSkills.GetAll(),
                productionDomainContext.ResourceSkills, productionDomainContext);

            DbTransactionData.InsertRange(_resourceTools.GetAll(),
                productionDomainContext.ResourceTools, productionDomainContext);

            DbTransactionData.InsertRange(_resourceSetups.GetAll(),
                productionDomainContext.ResourceSetups, productionDomainContext);

            DbTransactionData.InsertRange(_operations.GetAll(), productionDomainContext.Operations,
                productionDomainContext);

            DbTransactionData.InsertRange(_stocks.GetAll(), productionDomainContext.Stocks,
                productionDomainContext);

            DbTransactionData.InsertRange(_units.GetAll(), productionDomainContext.Units,
                productionDomainContext);
            
            productionDomainContext.SaveChanges();
        }

        public List<M_BusinessPartner> M_BusinessPartnerGetAll()
        {
            return _businessPartners.GetAll();
        }

        public M_ArticleBom M_ArticleBomGetById(Id id)
        {
            return _articleBoms.GetById(id);
        }
        
        public M_Article M_ArticleGetById(Id id)
        {
            return _articles.GetById(id);
        }

        public M_ArticleToBusinessPartner M_ArticleToBusinessPartnerGetById(Id id)
        {
            return _articleToBusinessPartners.GetById(id);
        }

        public M_ArticleType M_ArticleTypeGetById(Id id)
        {
            return _articleTypes.GetById(id);
        }

        public Resource M_ResourceGetById(Id id)
        {
            return new Resource(_resources.GetById(id));
        }

        public List<Resource> ResourcesGetAllBySkillId(Id id)
        {
            var skill = _resourceSkills.GetById(id);
            var resourceIds = _resourceSetups.GetAll().Where(x => x.ResourceSkillId == skill.Id)
                .Select(x => x.ResourceId).ToList();
            var resourceList = new List<Resource>();
            _resources.GetAll().Where(x => resourceIds.Contains(x.Id)).ToList()
                .ForEach(e => resourceList.Add(new Resource(e)));
            return resourceList;
        }

        public M_ResourceSkill M_ResourceSkillById(Id id)
        {
            return _resourceSkills.GetById(id);
        }

        public M_ResourceTool M_ResourceToolGetById(Id id)
        {
            return _resourceTools.GetById(id);
        }

        public M_Operation M_OperationGetById(Id id)
        {
            return _operations.GetById(id);
        }

        public M_Stock M_StockGetById(Id id)
        {
            return _stocks.GetById(id);
        }

        public M_Unit M_UnitGetById(Id id)
        {
            return _units.GetById(id);
        }

        public BusinessPartners M_ArticleToBusinessPartnerGetAllBusinessPartnersByArticleId(
            Id articleId)
        {
            List<M_BusinessPartner> businessPartners = new List<M_BusinessPartner>();
            foreach (var articleToBusinessPartner in _articleToBusinessPartners)
            {
                if (articleToBusinessPartner.ArticleId.Equals(articleId))
                {
                    M_BusinessPartner businessPartner =
                        _businessPartners.GetById(
                            new Id(articleToBusinessPartner.BusinessPartnerId));
                    businessPartners.Add(businessPartner);
                }
            }

            return new BusinessPartners(businessPartners);
        }

        public M_BusinessPartner M_BusinessPartnerGetById(Id id)
        {
            return _businessPartners.GetById(id);
        }

        public List<M_ArticleToBusinessPartner> M_ArticleToBusinessPartnerGetAllByArticleId(
            Id articleId)
        {
            List<M_ArticleToBusinessPartner> articleToBusinessPartners =
                new List<M_ArticleToBusinessPartner>();
            foreach (var articleToBusinessPartner in _articleToBusinessPartners)
            {
                if (articleToBusinessPartner.ArticleId.Equals(articleId.GetValue()))
                {
                    articleToBusinessPartners.Add(articleToBusinessPartner);
                }
            }

            return articleToBusinessPartners;
        }

        public M_Stock M_StockGetByArticleId(Id articleId)
        {
            M_Article article = M_ArticleGetById(articleId);
            if (article.Stock == null)
            {
                // init stocks for all articles
                foreach (var stock in _stocks)
                {
                    M_ArticleGetById(new Id(stock.ArticleForeignKey)).Stock = stock;
                }
            }

            return article.Stock;
        }

        public List<M_Stock> M_StockGetAll()
        {
            return _stocks.GetAll();
        }

        public List<M_Article> M_ArticleGetAll()
        {
            return _articles.GetAll();
        }

        public List<M_Article> M_ArticleGetArticlesToBuy()
        {
            List<M_Article> articlesToBuy = new List<M_Article>();
            M_ArticleType articleType = _articleTypes.GetAll()
                .Single(x => x.Name == MasterTableArticle.ARTICLE_PRODUCTS);

            articlesToBuy = _articles.GetAll().Where(x => x.ArticleTypeId == articleType.Id)
                .ToList();
            return articlesToBuy;
        }

        public M_Article M_ArticleGetByName(string name)
        {
            return _articles.GetAll().SingleOrDefault(x => String.Equals(x.Name, name,
                StringComparison.OrdinalIgnoreCase));
        }

        public void M_StockSetAll(List<M_Stock> stocks)
        {
            _stocks.SetAll(stocks);
        }

        public List<M_ResourceSetup> M_ResourceSetupGetAll()
        {
            return _resourceSetups.GetAll();
        }

        public List<Resource> ResourceGetAll()
        {
            List<Resource> machines = new List<Resource>();
            foreach (var machine in _resources)
            {
                machines.Add(new Resource(machine));
            }

            return machines;
        }

        public List<M_ResourceSkill> M_ResourceSkillGetAll()
        {
            return _resourceSkills.GetAll();
        }

        public M_ArticleBom M_ArticleBomGetByArticleChildId(Id id)
        {
            return _articleBoms.GetAll().Single(x => x.ArticleChildId.Equals(id.GetValue()));
        }
    }
}