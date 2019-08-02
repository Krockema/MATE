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
using Zpp.Utils;

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
        private IProviderManager _providerManager;

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

        public DbTransactionData(ProductionDomainContext productionDomainContext,
            IDbMasterDataCache dbMasterDataCache)
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

            _productionOrderBoms =
                new ProductionOrderBoms(_productionDomainContext.ProductionOrderBoms.ToList(),
                    _dbMasterDataCache);

            _stockExchangeDemands =
                new StockExchangeDemands(_productionDomainContext.StockExchanges.ToList(),
                    _dbMasterDataCache);
            _stockExchangeProviders =
                new StockExchangeProviders(_productionDomainContext.StockExchanges.ToList(),
                    _dbMasterDataCache);

            _productionOrders =
                new ProductionOrders(_productionDomainContext.ProductionOrders.ToList(),
                    _dbMasterDataCache);
            _purchaseOrderParts =
                new PurchaseOrderParts(_productionDomainContext.PurchaseOrderParts.ToList(),
                    _dbMasterDataCache);

            IDemandToProviderTable demandToProviderTable =
                new DemandToProviderTable(_productionDomainContext.DemandToProviders.ToList());
            IProviderToDemandTable providerToDemandTable =
                new ProviderToDemandTable(_productionDomainContext.ProviderToDemand.ToList());

            IProviders providers = new Providers();
            providers.AddAll(_purchaseOrderParts);
            providers.AddAll(_productionOrders);
            providers.AddAll(_stockExchangeProviders);
            _providerManager = new ProviderManager(demandToProviderTable, providerToDemandTable, providers);
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

        public void PersistDbCache(IProviderManager providerManager)
        {
            _providerManager = providerManager;
            
            // TODO: performance issue: Batch insert, since those T_* didn't exist before anyways, update is useless
            // TODO: SaveChanges at the end only once

            // first collect all T_* entities
            List<T_ProductionOrderBom> tProductionOrderBoms =
                _productionOrderBoms.GetAllAs<T_ProductionOrderBom>();
            List<T_StockExchange> tStockExchangeDemands =
                _stockExchangeDemands.GetAllAs<T_StockExchange>();
            List<T_StockExchange> tStockExchangesProviders =
                _stockExchangeProviders.GetAllAs<T_StockExchange>();
            List<T_ProductionOrder> tProductionOrders =
                _productionOrders.GetAllAs<T_ProductionOrder>();
            List<T_PurchaseOrderPart> tPurchaseOrderParts =
                _purchaseOrderParts.GetAllAs<T_PurchaseOrderPart>();

            // T_ProductionOrderOperation
            List<T_ProductionOrderOperation> tProductionOrderOperations =
                new List<T_ProductionOrderOperation>();
            foreach (var tProductionOrderBom in tProductionOrderBoms)
            {
                tProductionOrderOperations.Add(tProductionOrderBom.ProductionOrderOperation);
            }

            // T_PurchaseOrders
            List<T_PurchaseOrder> tPurchaseOrders = new List<T_PurchaseOrder>();
            foreach (var tPurchaseOrderPart in tPurchaseOrderParts)
            {
                tPurchaseOrders.Add(tPurchaseOrderPart.PurchaseOrder);
            }

            // Insert all T_* entities

            InsertOrUpdateRange(tProductionOrderBoms, _productionDomainContext.ProductionOrderBoms);
            InsertOrUpdateRange(tProductionOrderOperations,
                _productionDomainContext.ProductionOrderOperations);
            InsertOrUpdateRange(tStockExchangeDemands, _productionDomainContext.StockExchanges);
            // providers
            InsertOrUpdateRange(tStockExchangesProviders, _productionDomainContext.StockExchanges);
            InsertOrUpdateRange(tProductionOrders, _productionDomainContext.ProductionOrders);
            InsertOrUpdateRange(tPurchaseOrderParts, _productionDomainContext.PurchaseOrderParts);
            InsertOrUpdateRange(tPurchaseOrders, _productionDomainContext.PurchaseOrders);

            // at the end: T_DemandToProvider & T_ProviderToDemand
            InsertOrUpdateRange(_providerManager.GetDemandToProviderTable().GetAll(),
                _productionDomainContext.DemandToProviders);
            InsertOrUpdateRange(_providerManager.GetProviderToDemandTable().GetAll(),
                _productionDomainContext.ProviderToDemand);

            try
            {
                _productionDomainContext.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Error("DbCache could not be persisted.");
                throw e;
            }
        }

        private void InsertOrUpdateRange<TEntity>(List<TEntity> entities, DbSet<TEntity> dbSet)
            where TEntity : BaseEntity
        {
            if (entities.Any() == false)
            {
                throw new MrpRunException("Collection to persist is empty.");
            }

            // dbSet.AddRange(entities);
            foreach (var entity in entities)
            {
                // e.g. if it is a PrBom which is toPurchase
                if (entity != null)
                {
                    InsertOrUpdate(entity, dbSet);
                }
            }
        }

        private void InsertOrUpdate<TEntity>(TEntity entity, DbSet<TEntity> dbSet)
            where TEntity : BaseEntity
        {
            TEntity foundEntity = dbSet.Find(entity.Id);
            if (foundEntity == null
                ) // TODO: performance issue: a select before every insert is a no go
                // it's not in DB yet
            {
                _productionDomainContext.Entry(entity).State = EntityState.Added;
                dbSet.Add(entity);
            }
            else
                // it's already in DB
            {
                CopyProperties(entity, foundEntity);
                _productionDomainContext.Entry(foundEntity).State = EntityState.Modified;
                dbSet.Update(foundEntity);
            }
        }


        private static void CopyProperties<T, TU>(T source, TU destination)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties().Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    {
                        // check if the property can be set or no.
                        p.SetValue(destination, sourceProp.GetValue(source, null), null);
                    }
                }
            }
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

        public IDemands DemandsGetAll()
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

            if (_dbMasterDataCache.T_CustomerOrderGetAll().Any())
            {
                demands.AddAll(_dbMasterDataCache.T_CustomerOrderPartGetAll());
            }

            return demands;
        }


        public IProviders ProvidersGetAll()
        {
            IProviders providers = new Providers();
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

        public void DemandsAddAll(IDemands demands)
        {
            foreach (var demand in demands.GetAll())
            {
                DemandsAdd(demand);
            }
        }

        public void ProvidersAddAll(IProviders providers)
        {
            foreach (var provider in providers.GetAll())
            {
                ProvidersAdd(provider);
            }
        }

        public IDemandToProviderTable DemandToProviderGetAll()
        {
            return _providerManager.GetDemandToProviderTable();
        }

        public Demand DemandsGetById(Id id)
        {
            return DemandsGetAll().GetAll().Find(x => x.GetId().Equals(id));
        }

        public Provider ProvidersGetById(Id id)
        {
            return ProvidersGetAll().GetAll().Find(x => x.GetId().Equals(id));
        }

        public IProviderToDemandTable ProviderToDemandGetAll()
        {
            return _providerManager.GetProviderToDemandTable();
        }

        public IProviderManager GetProviderManager()
        {
            return _providerManager;
        }
    }
}