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
using Zpp.LotSize;

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

        public Provider CreateProvider(IDbTransactionData dbTransactionData, Quantity quantity)
        {
            M_Article article = GetArticle();

            // make or buy
            ILotSize lotSize = new LotSize.LotSize(quantity, GetArticleId());
            if (article.ToBuild)
            {
                ProductionOrder productionOrder =
                    ProductionOrder.CreateProductionOrder(this, dbTransactionData,
                        _dbMasterDataCache, lotSize);
                Logger.Debug("ProductionOrder created.");
                return productionOrder;
            }

            // TODO: revisit all methods that have this as parameter
            return PurchaseOrderPart.CreatePurchaseOrderPart(this, lotSize, _dbMasterDataCache);
        }


        // TODO: use this method
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

        public Providers Satisfy(IDemandToProviders demandToProviders,
            IDbTransactionData dbTransactionData, Demands nextDemands)
        {
            Providers providers = new Providers();

            // use existing provider, if one is not exhausted
            Provider nonExhaustedProvider = demandToProviders.FindNonExhaustedProvider(GetArticle());
            Quantity unsatisfiedQuantity = GetQuantity();
            Quantity nullQuantity = new Quantity(0);
            if (nonExhaustedProvider != null)
            {
                unsatisfiedQuantity = unsatisfiedQuantity.Minus(nonExhaustedProvider.GetQuantity());
                providers.Add(nonExhaustedProvider);
                if (unsatisfiedQuantity.Equals(nullQuantity))
                {
                    return providers;
                }
            }

            // satisfy by stock if possible
            Provider stockProvider = StockExchangeProvider.CreateStockProvider(GetArticle(),
                GetDueTime(), unsatisfiedQuantity, _dbMasterDataCache, dbTransactionData);
            if (stockProvider != null)
            {
                unsatisfiedQuantity.Minus(stockProvider.GetQuantity());
                providers.Add(stockProvider);
                if (stockProvider.AnyDemands())
                {
                    // TODO: This should do the caller, but then the caller must get providers and nextDemands...
                    nextDemands.AddAll(stockProvider.GetDemands());
                }
                
                if (unsatisfiedQuantity.Equals(nullQuantity))
                {
                    return providers;
                }
            }

            // satisfy by provider
            Logger.Debug($"Create a provider for article {this}:");

            Provider createdProvider = CreateProvider(dbTransactionData, unsatisfiedQuantity);
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

        public Id GetT_DemandId()
        {
            return new Id(_demand.DemandID);
        }
    }
}