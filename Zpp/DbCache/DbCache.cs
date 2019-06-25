using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;
using Zpp;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public class DbCache : IDbCache
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        protected readonly ProductionDomainContext _productionDomainContext;

        // cached tables
        // M_*
        private readonly List<M_BusinessPartner> _businessPartners;
        private readonly List<M_ArticleBom> _articleBoms;
        private readonly List<M_Article> _articles;
        // T_*
        // demands
        private readonly CustomerOrderParts _customerOrderParts;
        // demands
        private readonly ProductionOrderBoms _productionOrderBoms;
        // demands
        private readonly StockExchangeDemands _stockExchangeDemands;
        // providers
        private readonly StockExchangeProviders _stockExchangeProviders;
        // providers
        private readonly PurchaseOrderParts _purchaseOrderParts;
        // providers
        private readonly ProductionOrders _productionOrders;
        
        private readonly PurchaseOrders _purchaseOrders;


        private readonly Dictionary<BaseEntity, bool> _wasTableChanged = new Dictionary<BaseEntity, bool>();

        public DbCache(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;

            // cache tables
            _businessPartners = _productionDomainContext.BusinessPartners.ToList();
            _articleBoms = _productionDomainContext.ArticleBoms.Include(m => m.ArticleChild)
                .ToList();
            _articles = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild)
                .Include(m => m.ArticleBoms).ThenInclude(x=>x.Operation)
                .Include(x=>x.ArticleToBusinessPartners).ThenInclude(x=>x.BusinessPartner).ToList();

            
            List<T_CustomerOrderPart> customerOrderParts = _productionDomainContext.CustomerOrderParts.Include(x => x.Article)
                .Include(x => x.CustomerOrder).ToList();
            _customerOrderParts = new CustomerOrderParts(customerOrderParts);
            List<T_ProductionOrderBom> productionOrderBoms = _productionDomainContext.ProductionOrderBoms.ToList();
            _productionOrderBoms = new ProductionOrderBoms(productionOrderBoms);
            List<T_StockExchange> stockExchanges = _productionDomainContext.StockExchanges.ToList();
            _stockExchangeDemands = new StockExchangeDemands(stockExchanges);
            _stockExchangeProviders = new StockExchangeProviders(stockExchanges);
            List<T_ProductionOrder> productionOrders = _productionDomainContext.ProductionOrders.Include(x => x.Article).ToList();
            _productionOrders = new ProductionOrders(productionOrders);
            List<T_PurchaseOrderPart> purchaseOrderParts = _productionDomainContext.PurchaseOrderParts.Include(x => x.Article).ToList();
            _purchaseOrderParts = new PurchaseOrderParts(purchaseOrderParts);
            List<T_PurchaseOrder> purchaseOrders = _productionDomainContext.PurchaseOrders.ToList();
            _purchaseOrders = new PurchaseOrders(purchaseOrders);
        }

        public void DemandToProvidersRemoveAll()
        {
            _productionDomainContext.DemandToProviders.RemoveRange(_productionDomainContext
                .DemandToProviders);
        }

        public void PersistDbCache()
        {

            // this should stay here, since all domainContext should only used here
            // InsertOrUpdateRange(_customerOrderParts, _productionDomainContext.CustomerOrderParts);
            InsertOrUpdateRange(_productionOrderBoms.GetAllAs<T_ProductionOrderBom>(), _productionDomainContext.ProductionOrderBoms);
            InsertOrUpdateRange(_stockExchangeDemands.GetAllAs<T_StockExchange>(), _productionDomainContext.StockExchanges);
            InsertOrUpdateRange(_productionOrders.GetAllAs<T_ProductionOrder>(), _productionDomainContext.ProductionOrders);
            InsertOrUpdateRange(_purchaseOrderParts.GetAllAs<T_PurchaseOrderPart>(), _productionDomainContext.PurchaseOrderParts);
            
            InsertOrUpdateRange(_purchaseOrders.GetAllAsT_PurchaseOrder(), _productionDomainContext.PurchaseOrders);
            
            _productionDomainContext.SaveChanges();
        }
        
        private void InsertOrUpdateRange<TEntity>(List<TEntity> entities, DbSet<TEntity> dbSet)  where TEntity : BaseEntity
        {
            // dbSet.AddRange(entities);
            foreach (var entity in entities)
            {
                InsertOrUpdate(entity, dbSet);
            }
        }

        private void InsertOrUpdate<TEntity>(TEntity entity, DbSet<TEntity> dbSet)  where TEntity : BaseEntity
        {
            if (entity.Id.Equals(0))
            // it's not in DB yet
            {
                dbSet.Add(entity);
            }
            else
            {
                dbSet.Update(entity);
            }
        }

        public void PurchaseOrderAdd(PurchaseOrder purchaseOrder)
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


   
        public void DemandsAdd(Demand demand)
        {
            if (demand.GetType() == typeof(CustomerOrderPart))
            {
                _customerOrderParts.Add((CustomerOrderPart)demand);
            }
            else if (demand.GetType() == typeof(ProductionOrderBom))
            {
                    _productionOrderBoms.Add((ProductionOrderBom)demand);
            }
            else if (demand.GetType() == typeof(StockExchangeDemand))
            {
                    _stockExchangeDemands.Add((StockExchangeDemand)demand);
            }
            else
            {
                LOGGER.Error("Unknown type implementing Demand");
            }
        }

        public void ProvidersAdd(Provider provider)
        {
            if (provider.GetType() == typeof(ProductionOrder))
            {
                _productionOrders.Add((ProductionOrder)provider);
            }
            else if (provider.GetType() == typeof(PurchaseOrderPart))
            {
                _purchaseOrderParts.Add((PurchaseOrderPart)provider);
            }
            else if (provider.GetType() == typeof(StockExchangeProvider))
            {
                _stockExchangeProviders.Add((StockExchangeProvider)provider);
            }
            else
            {
                LOGGER.Error("Unknown type implementing IProvider");
            }
        }

        public Demands DemandsGetAll()
        {
            Demands demands = new Demands();
            demands.AddAll(_customerOrderParts);
            demands.AddAll(_productionOrderBoms);
            demands.AddAll(_stockExchangeDemands);
            return demands;
        }


        public Providers ProvidersGetAll()
        {
            Providers providers = new Providers();
            providers.AddAll(_productionOrders);
            providers.AddAll(_purchaseOrderParts);
            providers.AddAll(_stockExchangeProviders);
            return providers;
        }

        public CustomerOrderParts CustomerOrderPartGetAll()
        {
            return _customerOrderParts;
        }

        public ProductionOrderBoms ProductionOrderBomGetAll()
        {
            return _productionOrderBoms;
        }

        public StockExchangeProviders StockExchangeGetAll()
        {
            return _stockExchangeProviders;
        }

        public PurchaseOrderParts PurchaseOrderPartGetAll()
        {
            return _purchaseOrderParts;
        }

        public ProductionOrders ProductionOrderGetAll()
        {
            return _productionOrders;
        }
    }
}