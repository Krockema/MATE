using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore;

namespace Zpp
{
    public class DbCache : IDbCache
    {
        protected readonly ProductionDomainContext _productionDomainContext;
        
        // cached tables
        private readonly List<M_BusinessPartner> _businessPartners;
        private readonly List<M_ArticleBom> _articleBoms;
        private readonly List<M_Article> _articles;
        
        public DbCache(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;
            
            // cache tables
            _businessPartners = _productionDomainContext.BusinessPartners.ToList();
            _articleBoms = _productionDomainContext.ArticleBoms.Include(m => m.ArticleChild).ToList();
            _articles = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).ToList();
        }

        public void T_DemandToProvidersRemoveAll()
        {
            _productionDomainContext.DemandToProviders.RemoveRange(_productionDomainContext
                .DemandToProviders);
        }

        public void persistDbCache()
        {
            _productionDomainContext.SaveChanges();
        }

        public void T_PurchaseOrderAdd(T_PurchaseOrder purchaseOrder)
        {
            _productionDomainContext.PurchaseOrders.Add(purchaseOrder);
        }

        public List<M_BusinessPartner> M_BusinessPartnerGetAll()
        {
            return _businessPartners;
        }

        public M_ArticleBom M_ArticleBomGetById(int id)
        {
            return _articleBoms.Single(x => x.Id == id);
        }

        public M_Article M_ArticleGetById(int id)
        {
            return _articles.Single(x => x.Id == id);
        }
    }
}