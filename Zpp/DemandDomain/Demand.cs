using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.ProviderDomain;
using ZppForPrimitives;

namespace Zpp.DemandDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Demand : IDemandLogic
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly IDemand _demand;
        protected readonly List<Provider> _providers;
        protected readonly Guid _guid = new Guid();

        public Demand(IDemand demand)
        {
            _demand = demand;
        }
        
        public Demand()
        {
        }
        
        public Provider CreateProvider(IDbCache dbCache)
        {
            if (_demand.GetArticle().ToBuild)
            {
                T_ProductionOrder productionOrder =  new T_ProductionOrder(_demand);
                Logger.Debug("ProductionOrder created.");
                Demands productionOrderBoms = ProcessArticleBoms(_demand, productionOrder, dbCache);
                return new ProductionOrder(productionOrder, productionOrderBoms);
            }
            return createPurchaseOrderPart(_demand);
        }
        
        private Provider createPurchaseOrderPart(IDemand demand)
        {
            // currently only one businessPartner per article
            M_ArticleToBusinessPartner articleToBusinessPartner = demand.GetArticle()
                .ArticleToBusinessPartners.OfType<M_ArticleToBusinessPartner>().First();
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = demand.GetDueTime();
            purchaseOrder.BusinessPartner = articleToBusinessPartner.BusinessPartner;
            purchaseOrder.Name = $"PurchaseOrder{demand.GetArticle().Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";


            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > demand.GetDueTime())
            {
                Logger.Error($"Article {demand.GetArticle().Id} from demand {demand.Id} " +
                             $"should be available at {demand.GetDueTime()}, but " +
                             $"businessPartner {articleToBusinessPartner.BusinessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = demand.GetArticle();
            purchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner,
                    demand.GetQuantity());
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();


            Logger.Debug("PurchaseOrderPart created.");
            return new PurchaseOrderPart(purchaseOrderPart, null);
        }
        
        private Demands ProcessArticleBoms(IDemand demand,
            T_ProductionOrder productionOrder, IDbCache dbCache)
        {
            M_Article readArticle = dbCache.M_ArticleGetById(demand.GetArticle().Id);
            if (readArticle.ArticleBoms != null && readArticle.ArticleBoms.Any())
            {
                List<Demand> productionOrderBoms = new List<Demand>();
                foreach (M_ArticleBom articleBom in readArticle.ArticleBoms)
                {
                    ProductionOrderBom productionOrderBom = new ProductionOrderBom(articleBom,
                        productionOrder);
                    productionOrderBoms.Add(productionOrderBom);
                }

                return new ProductionOrderBoms(productionOrderBoms);
            }

            return null;
        }

        // TODO: use this
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
        }
        
        public DueTime GetDueTime()
        {
            return new DueTime(_demand.GetDueTime());
        }
        
        public static Demand ToDemand(T_Demand t_demand, List<T_CustomerOrderPart> customerOrderParts,
            List<T_ProductionOrderBom> productionOrderBoms, List<T_StockExchange> stockExchanges)
        {
            IDemand iDemand = null;

            iDemand = customerOrderParts.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return new CustomerOrderPart(iDemand);
            }

            iDemand = productionOrderBoms.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return new ProductionOrderBom(iDemand);
            }

            iDemand = stockExchanges.Single(x => x.Id == t_demand.Id);
            if (iDemand != null)
            {
                return new StockExchangeDemand(iDemand);
            }

            return null;
        }

        public abstract IDemand ToIDemand();

        public void AddProvider(Provider provider)
        {
            _providers.Add(provider);
        }

        public bool HasProvider(Providers providers)
        {
            throw new NotImplementedException();
        }
        
        public override bool Equals(object obj)
        {
            var item = obj as Demand;

            if (item == null)
            {
                return false;
            }

            return _guid.Equals(item._guid);
        }
        
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public Quantity GetQuantity()
        {
            return _demand.GetQuantity();
        }

        public override string ToString()
        {
            return $"{_demand.Id}: {_demand.GetQuantity()} of {_demand.GetArticle().Name}";
        }
    }
}