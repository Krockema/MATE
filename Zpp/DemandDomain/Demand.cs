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
        protected readonly IDbCacheMasterData _dbCacheMasterData;

        public Demand(IDemand demand, IDbCacheMasterData dbCacheMasterData)
        {
            if (demand == null)
            {
                throw new MrpRunException("Given demand should not be null.");
            }

            _demand = demand;
            _dbCacheMasterData = dbCacheMasterData;
        }

        public Provider CreateProvider(IDbTransactionData dbTransactionData)
        {
            M_Article article = GetArticle();
            if (article.ToBuild)
            {
                ProductionOrder productionOrder =
                    new ProductionOrder(this, dbTransactionData, _dbCacheMasterData);
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
                _dbCacheMasterData.M_ArticleToBusinessPartnerGetAllByArticleId(
                    demand.GetArticle().GetId())[0];
            M_BusinessPartner businessPartner =
                _dbCacheMasterData.M_BusinessPartnerGetById(new Id(articleToBusinessPartner
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
                    demand.GetQuantity());
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();


            Logger.Debug("PurchaseOrderPart created.");
            return new PurchaseOrderPart(purchaseOrderPart, null);
        }


        // TODO: use this
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
        }

        public abstract IDemand ToIDemand();

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
            return $"{_demand.Id}: {_demand.GetQuantity()} of {_demand.Id}";
        }

        public abstract M_Article GetArticle();

        public abstract DueTime GetDueTime();

        public Id GetId()
        {
            return new Id(_demand.Id);
        }
    }
}