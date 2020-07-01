using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.ProviderDomain;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.DataLayer.impl.WrappersForCollections;
using Zpp.Util;
using Zpp.Util.StackSet;

namespace Zpp.DataLayer.impl
{
    /**
     * NOTE: TransactionData does NOT include CustomerOrders or CustomerOrderParts !
     */
    public class DbTransactionData : IDbTransactionData
    {
        private readonly ProductionDomainContext _productionDomainContext;

        // TODO: This line should be removed
        private readonly List<M_Article> _articles;

        // T_*

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

        // others
        private readonly IStackSet<T_PurchaseOrder> _purchaseOrders =
            new StackSet<T_PurchaseOrder>();

        private readonly ProductionOrderOperations _productionOrderOperations;

        private readonly CustomerOrderParts _customerOrderParts;

        private readonly CustomerOrders _customerOrders;

        private readonly LinkDemandAndProviderTable _demandToProviderTable;

        private readonly LinkDemandAndProviderTable _providerToDemandTable;

        public DbTransactionData(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;

            // cache tables
            // TODO: This line should be removed
            _articles = _productionDomainContext.Articles.Include(m => m.ArticleBoms)
                .ThenInclude(m => m.ArticleChild).Include(m => m.ArticleBoms)
                    .ThenInclude(x => x.Operation).ThenInclude(x => x.ResourceCapability)
                        .ThenInclude(s => s.ResourceCapabilityProvider).ThenInclude(r => r.ResourceSetups)
                            .ThenInclude(x => x.Resource)
                .Include(x => x.ArticleToBusinessPartners).ThenInclude(x => x.BusinessPartner)
                .ToList();

            _productionOrderBoms =
                new ProductionOrderBoms(_productionDomainContext.ProductionOrderBoms.ToList());

            _stockExchangeDemands =
                new StockExchangeDemands(_productionDomainContext.StockExchanges.ToList());
            _stockExchangeProviders =
                new StockExchangeProviders(_productionDomainContext.StockExchanges.ToList());

            _productionOrders =
                new ProductionOrders(_productionDomainContext.ProductionOrders.ToList());
            _purchaseOrderParts =
                new PurchaseOrderParts(_productionDomainContext.PurchaseOrderParts.ToList());

            _customerOrderParts =
                new CustomerOrderParts(_productionDomainContext.CustomerOrderParts.ToList());
            _customerOrders = new CustomerOrders(_productionDomainContext.CustomerOrders.ToList());

            // others
            _purchaseOrders.PushAll(_productionDomainContext.PurchaseOrders.ToList());
            _productionOrderOperations = new ProductionOrderOperations(
                _productionDomainContext.ProductionOrderOperations.ToList());

            // demandToProvider

            _demandToProviderTable =
                new LinkDemandAndProviderTable(_productionDomainContext.DemandToProviders);
            _providerToDemandTable =
                new LinkDemandAndProviderTable(_productionDomainContext.ProviderToDemand);
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

        internal void PersistDbCache()
        {
            // TODO: performance issue: Batch insert

            // first collect all T_* entities
            List<T_ProductionOrderBom> tProductionOrderBoms =
                _productionOrderBoms.GetAllAs<T_ProductionOrderBom>();
            List<T_StockExchange> tStockExchangeDemands =
                _stockExchangeDemands.GetAllAs<T_StockExchange>();
            List<T_StockExchange> tStockExchangesProviders =
                _stockExchangeProviders.GetAllAs<T_StockExchange>();
            List<T_ProductionOrder> tProductionOrders =
                _productionOrders.GetAllAs<T_ProductionOrder>();
            List<T_ProductionOrderOperation> tProductionOrderOperations =
                _productionOrderOperations.GetAllAsT_ProductionOrderOperation();
            List<T_PurchaseOrderPart> tPurchaseOrderParts =
                _purchaseOrderParts.GetAllAs<T_PurchaseOrderPart>();
            List<T_CustomerOrderPart> tCustomerOrderParts =
                _customerOrderParts.GetAllAs<T_CustomerOrderPart>();
            List<T_CustomerOrder> tCustomerOrders = _customerOrders.GetAllAsTCustomerOrders();

            // Insert all T_* entities
            InsertOrUpdateRange(tProductionOrders, _productionDomainContext.ProductionOrders,
                _productionDomainContext);
            InsertOrUpdateRange(tProductionOrderOperations,
                _productionDomainContext.ProductionOrderOperations, _productionDomainContext);
            InsertOrUpdateRange(tProductionOrderBoms, _productionDomainContext.ProductionOrderBoms,
                _productionDomainContext);
            InsertOrUpdateRange(tStockExchangeDemands, _productionDomainContext.StockExchanges,
                _productionDomainContext);
            InsertOrUpdateRange(tCustomerOrders, _productionDomainContext.CustomerOrders,
                _productionDomainContext);
            InsertOrUpdateRange(tCustomerOrderParts, _productionDomainContext.CustomerOrderParts,
                _productionDomainContext);

            // providers
            InsertOrUpdateRange(tStockExchangesProviders, _productionDomainContext.StockExchanges,
                _productionDomainContext);
            InsertOrUpdateRange(tPurchaseOrderParts, _productionDomainContext.PurchaseOrderParts,
                _productionDomainContext);
            InsertOrUpdateRange(_purchaseOrders, _productionDomainContext.PurchaseOrders,
                _productionDomainContext);

            // at the end: T_DemandToProvider & T_ProviderToDemand
            InsertOrUpdateRange(DemandToProviderGetAll().Select(x => (T_DemandToProvider) x),
                _productionDomainContext.DemandToProviders, _productionDomainContext);
            if (ProviderToDemandGetAll().Any())
            {
                InsertOrUpdateRange(ProviderToDemandGetAll().Select(x => (T_ProviderToDemand) x),
                    _productionDomainContext.ProviderToDemand, _productionDomainContext);
            }

            _productionDomainContext.SaveChanges();
        }

        public void CustomerOrderAdd(T_CustomerOrder customerOrder)
        {
            _customerOrders.Add(customerOrder);
        }

        public static void InsertRange<TEntity>(IEnumerable<TEntity> entities, DbSet<TEntity> dbSet,
            ProductionDomainContext productionDomainContext) where TEntity : BaseEntity
        {
            foreach (var entity in entities)
            {
                // e.g. if it is a PrBom which is toPurchase
                if (entity != null)
                {
                    Insert(entity, dbSet, productionDomainContext);
                }
            }
        }

        public static void InsertOrUpdateRange<TEntity>(IEnumerable<TEntity> entities,
            DbSet<TEntity> dbSet, ProductionDomainContext productionDomainContext)
            where TEntity : BaseEntity
        {
            // dbSet.AddRange(entities);
            foreach (var entity in entities)
            {
                // e.g. if it is a PrBom which is toPurchase
                if (entity != null)
                {
                    InsertOrUpdate(entity, dbSet, productionDomainContext);
                }
            }
        }

        private static void Insert<TEntity>(TEntity entity, DbSet<TEntity> dbSet,
            ProductionDomainContext productionDomainContext) where TEntity : BaseEntity
        {
            productionDomainContext.Entry(entity).State = EntityState.Added;
            dbSet.Add(entity);
        }

        private static void InsertOrUpdate<TEntity>(TEntity entity, DbSet<TEntity> dbSet,
            ProductionDomainContext productionDomainContext) where TEntity : BaseEntity
        {
            TEntity foundEntity = dbSet.Find(entity.Id);
            if (foundEntity == null
                )
                // it's not in DB yet
            {
                productionDomainContext.Entry(entity).State = EntityState.Added;
                dbSet.Add(entity);
            }
            else
                // it's already in DB
            {
                CopyDbPropertiesTo(entity, foundEntity);
                productionDomainContext.Entry(foundEntity).State = EntityState.Modified;
                dbSet.Update(foundEntity);
            }
        }

        private static void CopyDbPropertiesTo<T>(T source, T dest)
        {
            DbUtils.CopyDbPropertiesTo(source, dest);
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
            else if (demand.GetType() == typeof(CustomerOrderPart))
            {
                _customerOrderParts.Add((CustomerOrderPart) demand);
            }
            else
            {
                throw new MrpRunException("This type is unknown.");
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
                throw new MrpRunException("This type is not known.");
            }
        }

        public Demands DemandsGetAll()
        {
            Demands demands = new Demands();

            if (_productionOrderBoms.Any())
            {
                demands.AddAll(_productionOrderBoms);
            }

            if (_stockExchangeDemands.Any())
            {
                demands.AddAll(_stockExchangeDemands);
            }

            if (_customerOrderParts.Any())
            {
                demands.AddAll(_customerOrderParts);
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

        public StockExchangeProviders StockExchangeProvidersGetAll()
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
            foreach (var demand in demands)
            {
                DemandsAdd(demand);
            }

            // T_ProductionOrderOperation
            IStackSet<ProductionOrderOperation> tProductionOrderOperations =
                new StackSet<ProductionOrderOperation>();
            foreach (var productionOrderBom in _productionOrderBoms)
            {
                T_ProductionOrderBom tProductionOrderBom =
                    (T_ProductionOrderBom) productionOrderBom.ToIDemand();
                if (tProductionOrderBom != null)
                {
                    ((ProductionOrderBom) productionOrderBom).EnsureOperationIsLoadedIfExists();
                    if (tProductionOrderBom.ProductionOrderOperation == null)
                    {
                        throw new MrpRunException(
                            "Every tProductionOrderBom must have an operation.");
                    }

                    tProductionOrderOperations.Push(new ProductionOrderOperation(
                        tProductionOrderBom.ProductionOrderOperation));
                }
            }

            _productionOrderOperations.AddAll(tProductionOrderOperations);
        }

        public void ProductionOrderOperationAdd(ProductionOrderOperation productionOrderOperation)
        {
            // this (productionOrderOperation was already added) can happen,
            // since an operation can be used multiple times for ProductionorderBoms
            if (_productionOrderOperations.Contains(productionOrderOperation) == false)
            {
                _productionOrderOperations.Add(productionOrderOperation);
            }
        }

        public void ProvidersAddAll(Providers providers)
        {
            foreach (var provider in providers)
            {
                ProvidersAdd(provider);
            }

            // T_PurchaseOrders
            foreach (var tPurchaseOrderPart in _purchaseOrderParts.GetAllAs<T_PurchaseOrderPart>())
            {
                PurchaseOrderAdd(tPurchaseOrderPart.PurchaseOrder);
            }
        }

        public void PurchaseOrderAdd(T_PurchaseOrder purchaseOrder)
        {
            _purchaseOrders.Push(purchaseOrder);
        }

        public void PurchaseOrderDelete(T_PurchaseOrder purchaseOrder)
        {
            _purchaseOrders.Remove(purchaseOrder);
        }

        public LinkDemandAndProviderTable DemandToProviderGetAll()
        {
            return _demandToProviderTable;
        }

        public Demand DemandsGetById(Id id)
        {
            Demand demand = _productionOrderBoms.GetById(id);
            if (demand == null)
            {
                demand = _stockExchangeDemands.GetById(id);
                if (demand == null)
                {
                    demand = _customerOrderParts.GetById(id);
                }
            }

            return demand;
        }

        public Provider ProvidersGetById(Id id)
        {
            Provider provider = _productionOrders.GetById(id);
            if (provider == null)
            {
                provider = _purchaseOrderParts.GetById(id);
                if (provider == null)
                {
                    provider = _stockExchangeProviders.GetById(id);
                }
            }

            return provider;
        }

        public LinkDemandAndProviderTable ProviderToDemandGetAll()
        {
            return _providerToDemandTable;
        }

        public T_PurchaseOrder PurchaseOrderGetById(Id id)
        {
            return _purchaseOrders.Single(x => x.Id.Equals(id.GetValue()));
        }

        public List<T_PurchaseOrder> PurchaseOrderGetAll()
        {
            return _purchaseOrders.GetAll();
        }

        public ProductionOrderOperation ProductionOrderOperationGetById(Id id)
        {
            return _productionOrderOperations.GetById(id);
        }

        public ProductionOrderOperations ProductionOrderOperationGetAll()
        {
            return _productionOrderOperations;
        }

        public StockExchangeDemands StockExchangeDemandsGetAll()
        {
            return _stockExchangeDemands;
        }

        public ProductionOrder ProductionOrderGetById(Id id)
        {
            return (ProductionOrder)_productionOrders.GetById(id);
        }

        public T_CustomerOrder CustomerOrderGetById(Id id)
        {
            if (_customerOrders.Any() == false)
            {
                return null;
            }
            return _customerOrders.Single(x => x.Id.Equals(id.GetValue()));
        }

        public List<T_CustomerOrder> CustomerOrderGetAll()
        {
            return _customerOrders.GetAll();
        }

        public Demands CustomerOrderPartGetAll()
        {
            Demands demands = new Demands();
            foreach (var demand in _customerOrderParts)
            {
                demands.Add(new CustomerOrderPart(demand.ToIDemand()));
            }

            return demands;
        }

        public void CustomerOrderPartAdd(T_CustomerOrderPart customerOrderPart)
        {
            _customerOrderParts.Add(new CustomerOrderPart(customerOrderPart));
        }

        public void DemandToProviderAdd(T_DemandToProvider demandToProvider)
        {
            _demandToProviderTable.Add(demandToProvider);
        }

        public void ProviderToDemandAddAll(LinkDemandAndProviderTable providerToDemandTable)
        {
            _providerToDemandTable.AddAll(providerToDemandTable);
        }

        public void Dispose()
        {
            _articles.Clear();
            _customerOrders.Clear();
            _productionOrders.Clear();
            _purchaseOrders.Clear();
            _customerOrderParts.Clear();
            _productionOrderBoms.Clear();
            _productionOrderOperations.Clear();
            _purchaseOrderParts.Clear();
            _stockExchangeDemands.Clear();
            _stockExchangeProviders.Clear();
            _demandToProviderTable.Clear();
            _providerToDemandTable.Clear();
        }

        public void AddAllFrom(EntityCollector otherEntityCollector)
        {
            if (otherEntityCollector.GetDemands().Any())
            {
                DemandsAddAll(otherEntityCollector.GetDemands());
            }

            if (otherEntityCollector.GetProviders().Any())
            {
                ProvidersAddAll(otherEntityCollector.GetProviders());
            }

            if (otherEntityCollector.GetDemandToProviderTable().Any())
            {
                _demandToProviderTable.AddAll(otherEntityCollector.GetDemandToProviderTable());
            }

            if (otherEntityCollector.GetLinkDemandAndProviderTable().Any())
            {
                _providerToDemandTable.AddAll(otherEntityCollector.GetLinkDemandAndProviderTable());
            }
        }

        public void StockExchangeProvidersDelete(StockExchangeProvider stockExchangeProvider)
        {
            _stockExchangeProviders.Remove(stockExchangeProvider);
        }

        public void DemandToProviderDelete(T_DemandToProvider demandToProvider)
        {
            _demandToProviderTable.Remove(demandToProvider);
        }

        public void ProviderToDemandDelete(T_ProviderToDemand providerToDemand)
        {
            _providerToDemandTable.Remove(providerToDemand);
        }

        public void DemandToProviderDeleteAll(IEnumerable<T_DemandToProvider> demandToProviders)
        {
            foreach (var demandToProvider in demandToProviders)
            {
                DemandToProviderDelete(demandToProvider);
            }
        }

        public void ProviderToDemandDeleteAll(IEnumerable<T_ProviderToDemand> providerToDemands)
        {
            foreach (var providerToDemand in providerToDemands)
            {
                ProviderToDemandDelete(providerToDemand);
            }
        }

        public void DeleteA(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider is Demand)
            {
                DemandsDelete((Demand) demandOrProvider);
            }
            else if (demandOrProvider is Provider)
            {
                ProvidersDelete((Provider) demandOrProvider);
            }
            else
            {
                throw new MrpRunException("This type is unknown.");
            }
        }

        public void DemandsDelete(Demand demand)
        {
            if (demand.GetType() == typeof(ProductionOrderBom))
            {
                _productionOrderBoms.Remove((ProductionOrderBom) demand);
            }
            else if (demand.GetType() == typeof(StockExchangeDemand))
            {
                _stockExchangeDemands.Remove((StockExchangeDemand) demand);
            }
            else if (demand.GetType() == typeof(CustomerOrderPart))
            {
                _customerOrderParts.Remove((CustomerOrderPart) demand);
            }
            else
            {
                throw new MrpRunException("This type is unknown.");
            }
        }

        public void ProvidersDelete(Provider provider)
        {
            if (provider.GetType() == typeof(ProductionOrder))
            {
                _productionOrders.Remove((ProductionOrder) provider);
            }
            else if (provider.GetType() == typeof(PurchaseOrderPart))
            {
                _purchaseOrderParts.Remove((PurchaseOrderPart) provider);
            }
            else if (provider.GetType() == typeof(StockExchangeProvider))
            {
                _stockExchangeProviders.Remove((StockExchangeProvider) provider);
            }
            else
            {
                throw new MrpRunException("This type is unknown.");
            }
        }

        public void ProductionOrderOperationDeleteAll(
            List<ProductionOrderOperation> productionOrderOperations)
        {
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                _productionOrderOperations.Remove(productionOrderOperation);
            }
        }

        public void ProductionOrderOperationDelete(
            ProductionOrderOperation productionOrderOperation)
        {
            _productionOrderOperations.Remove(productionOrderOperation);
        }

        public void ProductionOrderOperationAddAll(
            List<ProductionOrderOperation> productionOrderOperations)
        {
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                ProductionOrderOperationAdd(productionOrderOperation);
            }
        }

        public void DeleteAllFrom(List<IDemandOrProvider> demandOrProviders)
        {
            foreach (var demandOrProvider in demandOrProviders)
            {
                DeleteA(demandOrProvider);
            }
        }

        public void DeleteAllFrom(List<ILinkDemandAndProvider> demandAndProviderLinks)
        {
            foreach (var demandAndProviderLink in demandAndProviderLinks)
            {
                DeleteA(demandAndProviderLink);
            }
        }

        public void AddAllFrom(List<IDemandOrProvider> demandOrProviders)
        {
            foreach (var demandOrProvider in demandOrProviders)
            {
                AddA(demandOrProvider);
            }
        }

        public void AddA(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider is Demand)
            {
                DemandsAdd((Demand) demandOrProvider);
            }
            else if (demandOrProvider is Provider)
            {
                ProvidersAdd((Provider) demandOrProvider);
            }
            else
            {
                throw new MrpRunException("This type is not expected.");
            }
        }

        public void AddAllFrom(List<ILinkDemandAndProvider> demandOrProviders)
        {
            foreach (var demandOrProvider in demandOrProviders)
            {
                AddA(demandOrProvider);
            }
        }

        public void AddA(ILinkDemandAndProvider demandAndProviderLink)
        {
            if (demandAndProviderLink.GetType() == typeof(T_DemandToProvider))
            {
                DemandToProviderAdd((T_DemandToProvider) demandAndProviderLink);
            }
            else if (demandAndProviderLink.GetType() == typeof(T_ProviderToDemand))
            {
                ProviderToDemandAdd((T_ProviderToDemand) demandAndProviderLink);
            }
            else
            {
                throw new MrpRunException("This type is not expected.");
            }
        }

        public void DeleteA(ILinkDemandAndProvider demandAndProviderLink)
        {
            if (demandAndProviderLink.GetType() == typeof(T_DemandToProvider))
            {
                DemandToProviderDelete((T_DemandToProvider) demandAndProviderLink);
            }
            else if (demandAndProviderLink.GetType() == typeof(T_ProviderToDemand))
            {
                ProviderToDemandDelete((T_ProviderToDemand) demandAndProviderLink);
            }
            else
            {
                throw new MrpRunException("This type is not expected.");
            }
        }

        public void ProviderToDemandAdd(T_ProviderToDemand providerToDemand)
        {
            _providerToDemandTable.Add(providerToDemand);
        }

        public override string ToString()
        {
            string result = "";

            result += "_customerOrders:" + Environment.NewLine + _customerOrders.ToString() +
                      Environment.NewLine + Environment.NewLine + Environment.NewLine;
            result += "_customerOrderParts:" + Environment.NewLine +
                      _customerOrderParts.ToString() + Environment.NewLine + Environment.NewLine +
                      Environment.NewLine;
            result += "_demandToProviderTable:" + Environment.NewLine +
                      _demandToProviderTable.ToString() + Environment.NewLine +
                      Environment.NewLine + Environment.NewLine;
            result += "_productionOrderBoms:" + Environment.NewLine +
                      _productionOrderBoms.ToString() + Environment.NewLine + Environment.NewLine +
                      Environment.NewLine;
            result += "_productionOrderOperations:" + Environment.NewLine +
                      _productionOrderOperations.ToString() + Environment.NewLine +
                      Environment.NewLine + Environment.NewLine;
            result += "_productionOrders:" + Environment.NewLine + _productionOrders.ToString() +
                      Environment.NewLine + Environment.NewLine + Environment.NewLine;
            result += "_providerToDemandTable:" + Environment.NewLine +
                      _providerToDemandTable.ToString() + Environment.NewLine +
                      Environment.NewLine + Environment.NewLine;
            result += "_purchaseOrderParts:" + Environment.NewLine +
                      _purchaseOrderParts.ToString() + Environment.NewLine + Environment.NewLine +
                      Environment.NewLine;
            result += "_purchaseOrders:" + Environment.NewLine + _purchaseOrders.ToString() +
                      Environment.NewLine + Environment.NewLine + Environment.NewLine;
            result += "_stockExchangeDemands:" + Environment.NewLine +
                      _stockExchangeDemands.ToString() + Environment.NewLine + Environment.NewLine +
                      Environment.NewLine;
            result += "_stockExchangeProviders:" + Environment.NewLine +
                      _stockExchangeProviders.ToString() + Environment.NewLine +
                      Environment.NewLine + Environment.NewLine;

            return result;
        }

        public T_DemandToProvider DemandToProviderGetById(Id id)
        {
            return (T_DemandToProvider)_demandToProviderTable.GetById(id);
        }

        public T_ProviderToDemand ProviderToDemandGetById(Id id)
        {
            return (T_ProviderToDemand)_providerToDemandTable.GetById(id);
        }

        public Demand ProductionOrderBomGetById(Id id)
        {
            return _productionOrderBoms.GetById(id);
        }

        public void CustomerOrderPartAddAll(List<T_CustomerOrderPart> customerOrderParts)
        {
            foreach (var customerOrderPart in customerOrderParts)
            {
                CustomerOrderPartAdd(customerOrderPart);
            }
            
        }

        public void CustomerOrderAddAll(List<T_CustomerOrder> customerOrders)
        {
            _customerOrders.AddAll(customerOrders);
        }

        public void CustomerOrderDelete(T_CustomerOrder customerOrder)
        {
            _customerOrders.Remove(customerOrder);
        }
    }
}