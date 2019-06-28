using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;
using Zpp;
using Zpp.DemandToProviderDomain;

namespace Zpp
{
    /**
     * NOTE: TransactionData does NOT include CustomerOrders or CustomerOrderParts !
     */
    public class DbTransactionData : IDbTransactionData
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ProductionDomainContext _productionDomainContext;
        private readonly IDbMasterDataCache _dbMasterDataCache;

        // TODO: These 3 lines should be removed
        private readonly List<M_Article> _articles;
        private readonly List<M_ArticleBom> _articleBoms;
        private readonly List<M_BusinessPartner> _businessPartners;

        // T_*
        private readonly IDemandToProviderTable _demandToProviderTable = new DemandToProviderTable();
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


        private readonly Dictionary<BaseEntity, bool> _wasTableChanged =
            new Dictionary<BaseEntity, bool>();

        public DbTransactionData(ProductionDomainContext productionDomainContext, IDbMasterDataCache dbMasterDataCache)
        {
            _productionDomainContext = productionDomainContext;
            _dbMasterDataCache = dbMasterDataCache;

            // cache tables
            // TODO: These 3 lines should be removed
            _businessPartners = _productionDomainContext.BusinessPartners.ToList();
            _articleBoms = _productionDomainContext.ArticleBoms.Include(m => m.ArticleChild)
                .ToList();
            _articles = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).Include(m => m.ArticleBoms)
                .ThenInclude(x => x.Operation).ThenInclude(x => x.MachineGroup)
                .Include(x => x.ArticleToBusinessPartners).ThenInclude(x => x.BusinessPartner)
                .ToList();

            _productionOrderBoms = new ProductionOrderBoms( new List<T_ProductionOrderBom>(), _dbMasterDataCache);
            
            _stockExchangeDemands = new StockExchangeDemands(new List<T_StockExchange>(), _dbMasterDataCache);
            _stockExchangeProviders = new StockExchangeProviders(new List<T_StockExchange>(), _dbMasterDataCache);
            
            _productionOrders = new ProductionOrders(new List<T_ProductionOrder>(), _dbMasterDataCache);
            _purchaseOrderParts = new PurchaseOrderParts(new List<T_PurchaseOrderPart>(), _dbMasterDataCache);
            _purchaseOrders = new PurchaseOrders(new List<T_PurchaseOrder>());
        }

        public List<M_BusinessPartner> M_BusinessPartnerGetAll()
        {
            return _businessPartners;
        }

        public M_ArticleBom M_ArticleBomGetById(Id id)
        {
            return _articleBoms.Single(x => x.Id == id.GetValue());
        }

        public M_Article M_ArticleGetById(Id id)
        {
            return _articles.Single(x => x.Id == id.GetValue());
        }

        public void DemandToProvidersRemoveAll()
        {
            _productionDomainContext.DemandToProviders.RemoveRange(_productionDomainContext
                .DemandToProviders);
        }

        public void PersistDbCache()
        {
            // InsertOrUpdateRange(_customerOrderParts, _productionDomainContext.CustomerOrderParts);
            // --> readOnly

            // TODO: performance issue: Batch insert, since those T_* didn't exist before anyways, update is useless
            InsertOrUpdateRange(_productionOrderBoms.GetAllAs<T_ProductionOrderBom>(),
                _productionDomainContext.ProductionOrderBoms);
            InsertOrUpdateRange(_stockExchangeDemands.GetAllAs<T_StockExchange>(),
                _productionDomainContext.StockExchanges);
            InsertOrUpdateRange(_stockExchangeProviders.GetAllAs<T_StockExchange>(),
                _productionDomainContext.StockExchanges);
            InsertOrUpdateRange(_productionOrders.GetAllAs<T_ProductionOrder>(),
                _productionDomainContext.ProductionOrders);
            InsertOrUpdateRange(_purchaseOrderParts.GetAllAs<T_PurchaseOrderPart>(),
                _productionDomainContext.PurchaseOrderParts);

            InsertOrUpdateRange(_purchaseOrders.GetAllAsT_PurchaseOrder(),
                _productionDomainContext.PurchaseOrders);
            
            InsertOrUpdateRange(_demandToProviderTable.GetAll(), _productionDomainContext.DemandToProviders);

            _productionDomainContext.SaveChanges();
        }

        private void InsertOrUpdateRange<TEntity>(List<TEntity> entities, DbSet<TEntity> dbSet)
            where TEntity : BaseEntity
        {
            // dbSet.AddRange(entities);
            foreach (var entity in entities)
            {
                InsertOrUpdate(entity, dbSet);
            }
        }

        private void InsertOrUpdate<TEntity>(TEntity entity, DbSet<TEntity> dbSet)
            where TEntity : BaseEntity
        {
            if(dbSet.Find(entity.Id) == null) // TODO: performance issue: a select before every insert is a no go
            // if (entity.Id.Equals(0))
                // it's not in DB yet
            {
                _productionDomainContext.Entry(entity).State = EntityState.Added;
                dbSet.Add(entity);
            }
            else
            {
                _productionDomainContext.Entry(entity).State = EntityState.Modified;
                dbSet.Update(entity);
            }
        }

        public void PurchaseOrderAdd(PurchaseOrder purchaseOrder)
        {
            _purchaseOrders.Add(purchaseOrder);
        }


        public void DemandsAdd(Demand demand)
        {
            if (demand.GetType() == typeof(ProductionOrderBom))
            {
                _productionOrderBoms.Add((ProductionOrderBom) demand);
            }
            else if (demand.GetType() == typeof(StockExchangeDemand))
            {
                _stockExchangeDemands.Add((StockExchangeDemand) demand);
            }
            else
            {
                Logger.Error("Unknown type implementing Demand");
            }
        }

        public void ProvidersAdd(Provider provider)
        {
            if (provider.GetType() == typeof(ProductionOrder))
            {
                _productionOrders.Add((ProductionOrder) provider);
            }
            else if (provider.GetType() == typeof(PurchaseOrderPart))
            {
                _purchaseOrderParts.Add((PurchaseOrderPart) provider);
            }
            else if (provider.GetType() == typeof(StockExchangeProvider))
            {
                _stockExchangeProviders.Add((StockExchangeProvider) provider);
            }
            else
            {
                Logger.Error("Unknown type implementing IProvider");
            }
        }

        public Demands DemandsGetAll()
        {
            Demands demands = new Demands();

            if (_productionOrderBoms.GetAll().Any())
            {
                demands.AddAll(_productionOrderBoms);
            }

            if (_stockExchangeDemands.GetAll().Any())
            {
                demands.AddAll(_stockExchangeDemands);
            }

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

        public void DemandsAddAll(Demands demands)
        {
            foreach (var demand in demands.GetAll())
            {
                DemandsAdd(demand);
            }
        }

        public void ProvidersAddAll(Providers providers)
        {
            foreach (var provider in providers.GetAll())
            {
                ProvidersAdd(provider);
            }
        }

        public void DemandToProviderAddAll(IDemandToProviders demandToProviders)
        {

            _demandToProviderTable.AddAll(demandToProviders);
        }

        public IDemandToProviderTable DemandToProviderGetAll()
        {
            return _demandToProviderTable;
        }
    }
}