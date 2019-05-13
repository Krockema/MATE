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
        private readonly List<T_Demand> _tDemands;
        private readonly List<T_Provider> _tProviders;
        private readonly List<T_CustomerOrderPart> _customerOrderParts;
        private readonly List<T_ProductionOrderBom> _productionOrderBoms;
        private readonly List<T_StockExchange> _stockExchanges;
        private readonly List<T_PurchaseOrderPart> _purchaseOrderParts;
        private readonly List<T_ProductionOrder> _productionOrders;
        private readonly List<T_PurchaseOrder> _purchaseOrders;
        
        
        public DbCache(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;
            
            // cache tables
            _businessPartners = _productionDomainContext.BusinessPartners.ToList();
            _articleBoms = _productionDomainContext.ArticleBoms.Include(m => m.ArticleChild).ToList();
            _articles = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).ToList();
            _tDemands = _productionDomainContext.Demands.ToList();
            _customerOrderParts = _productionDomainContext.CustomerOrderParts.ToList();
            _productionOrderBoms = _productionDomainContext.ProductionOrderBoms.ToList();
            _stockExchanges = _productionDomainContext.StockExchanges.ToList();
            _productionOrders = _productionDomainContext.ProductionOrders.ToList();
            _purchaseOrderParts = _productionDomainContext.PurchaseOrderParts.ToList();
            _purchaseOrders = _productionDomainContext.PurchaseOrders.ToList();
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
            _purchaseOrders.Add(purchaseOrder);
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

        public List<T_Demand> T_DemandsGetAll()
        {
            return _tDemands;
        }

        public List<T_Provider> T_ProvidersGetAll()
        {
            return _tProviders;
        }

        public List<T_CustomerOrderPart> T_CustomerOrderPartGetAll()
        {
            return _customerOrderParts;
        }

        public List<T_ProductionOrderBom> T_ProductionOrderBomGetAll()
        {
            return _productionOrderBoms;
        }

        public List<T_StockExchange> T_StockExchangeGetAll()
        {
            return _stockExchanges;
        }

        public List<T_PurchaseOrderPart> T_PurchaseOrderPartGetAll()
        {
            return _purchaseOrderParts;
        }

        public List<T_ProductionOrder> T_ProductionOrderGetAll()
        {
            return _productionOrders;
        }
    }
}