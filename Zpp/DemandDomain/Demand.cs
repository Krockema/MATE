using Master40.DB.Data.WrappersForPrimitives;
using Zpp.ProviderDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
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

        public Providers CreateProviders(IDbTransactionData dbTransactionData, Quantity quantity)
        {
            M_Article article = GetArticle();
            Providers providers = new Providers();
            
            // make or buy
            if (article.ToBuild)
            {
                LotSize.LotSize lotSizes = new LotSize.LotSize(quantity, GetArticleId());
                foreach (var lotSize in lotSizes.GetCalculatedQuantity())
                {
                    ProductionOrder productionOrder =
                        ProductionOrder.CreateProductionOrder(this, dbTransactionData,
                            _dbMasterDataCache, lotSize);
                    Logger.Debug("ProductionOrder created.");
                    providers.Add(productionOrder);
                }

                return providers;
            }

            // TODO: revisit all methods that have this as parameter
            PurchaseOrderPart purchaseOrderPart =
                (PurchaseOrderPart) PurchaseOrderPart.CreatePurchaseOrderPart(GetId(), GetArticle(),
                    GetDueTime(), quantity, _dbMasterDataCache);
            Logger.Debug("PurchaseOrderPart created.");
            providers.Add(purchaseOrderPart);
            
            return providers;
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

            return _demand.Id.Equals(item._demand.Id);
        }

        public override int GetHashCode()
        {
            return _demand.Id.GetHashCode();
        }

        public Quantity GetQuantity()
        {
            return _demand.GetQuantity();
        }

        public override string ToString()
        {
            return $"{GetId()}: {GetQuantity()} pieces";
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
            Provider nonExhaustedProvider =
                demandToProviders.FindNonExhaustedProvider(GetArticle());
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
                if (stockProvider.AnyDependingDemands())
                {
                    // TODO: This should do the caller, but then the caller must get providers and nextDemands...
                    nextDemands.AddAll(stockProvider.GetAllDependingDemands());
                }

                if (unsatisfiedQuantity.Equals(nullQuantity))
                {
                    return providers;
                }
            }

            // satisfy by provider
            Logger.Debug($"Create a providers for article {this}:");

            Providers createdProviders = CreateProviders(dbTransactionData, unsatisfiedQuantity);
            providers.AddAll(createdProviders);
            // TODO: performance: this could be faster, if do adding dependingDemands directly instead of using Any
            if (createdProviders.AnyDependingDemands())
            {
                // TODO: This should do the caller, but then the caller must get providers and nextDemands...
                nextDemands.AddAll(createdProviders.GetAllDependingDemands());
            }

            return providers;
        }

        public T_Demand ToT_Demand()
        {
            if (_demand.Demand == null)
            {
                _demand.Demand =
                    _dbMasterDataCache.T_DemandGetById(
                        new Id(_demand.DemandId.GetValueOrDefault()));
            }

            return _demand.Demand;
        }

        public Id GetT_DemandId()
        {
            return new Id(_demand.DemandId.GetValueOrDefault());
        }
    }
}