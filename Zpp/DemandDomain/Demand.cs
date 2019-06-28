using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.ProviderDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.DemandToProviderDomain;

namespace Zpp.DemandDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Demand : IDemandLogic
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly IDemand _demand;
        protected readonly Guid _guid = Guid.NewGuid();
        protected readonly IDbMasterDataCache _dbMasterDataCache;

        public Demand(IDemand demand, IDbMasterDataCache dbMasterDataCache)
        {
            if (demand == null)
            {
                throw new MrpRunException("Given demand should not be null.");
            }

            _demand = demand;
            _dbMasterDataCache = dbMasterDataCache;
        }

        public Provider CreateProvider(IDbTransactionData dbTransactionData)
        {
            M_Article article = GetArticle();
            if (article.ToBuild)
            {
                ProductionOrder productionOrder =
                    new ProductionOrder(this, dbTransactionData, _dbMasterDataCache);
                Logger.Debug("ProductionOrder created.");
                return productionOrder;
            }

            // TODO: remove all parameters fo methos, where only "this" is given to it
            return createPurchaseOrderPart(this);
        }

        private Provider createPurchaseOrderPart(Demand demand)
        {
            // currently only one businessPartner per article TODO: This could be changing
            M_ArticleToBusinessPartner articleToBusinessPartner =
                _dbMasterDataCache.M_ArticleToBusinessPartnerGetAllByArticleId(
                    demand.GetArticle().GetId())[0];
            M_BusinessPartner businessPartner =
                _dbMasterDataCache.M_BusinessPartnerGetById(new Id(articleToBusinessPartner
                    .BusinessPartnerId));
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = GetDueTime().GetValue();
            purchaseOrder.BusinessPartner = businessPartner;
            purchaseOrder.Name = $"PurchaseOrder{GetArticle().Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";


            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > GetDueTime().GetValue())
            {
                Logger.Error($"Article {GetArticle().Id} from demand {demand.GetId()} " +
                             $"should be available at {GetDueTime()}, but " +
                             $"businessPartner {articleToBusinessPartner.BusinessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = GetArticle();
            purchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner,
                    demand.GetQuantity()) * articleToBusinessPartner.PackSize; // TODO: is amount*packSize in var quantity correct?
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();


            Logger.Debug("PurchaseOrderPart created.");
            return new PurchaseOrderPart(purchaseOrderPart, null, _dbMasterDataCache);
        }


        // TODO: use this
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
        }

        public abstract IDemand ToIDemand();

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
            return $"{_demand.Id}: {_demand.GetQuantity()} of {_demand.Id}";
        }

        public abstract M_Article GetArticle();

        public abstract DueTime GetDueTime();

        public Id GetId()
        {
            return new Id(_demand.Id);
        }

        public Id GetArticleId()
        {
            return new Id(GetArticle().Id);
        }

        public Providers Satisfy(IDemandToProviders demandToProviders, IDbTransactionData dbTransactionData, Demands nextDemands)
        {
            Providers providers = new Providers();
            Provider nonExhaustedProvider = demandToProviders.FindNonExhaustedProvider(this);
            if (nonExhaustedProvider != null)
            {
                providers.Add(nonExhaustedProvider);
                if (nonExhaustedProvider.ProvidesMoreThan(this.GetQuantity()))
                {
                    return providers;
                }
            }

            Logger.Debug($"Create a provider for article {this}:");

            Provider createdProvider = CreateProvider(dbTransactionData);
            providers.Add(createdProvider);
            if (createdProvider.AnyDemands())
            {
                // TODO: This should do the caller, but then the caller must get providers and nextDemands...
                nextDemands.AddAll(createdProvider.GetDemands());
            }

            return providers;
        }

        public T_Demand ToT_Demand()
        {
            return _demand.Demand;
        }
    }
}