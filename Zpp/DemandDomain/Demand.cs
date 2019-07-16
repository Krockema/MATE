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
    public abstract class Demand : IDemandLogic, INode
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

        public IProviders SatisfyByOrders(IDbTransactionData dbTransactionData, Quantity quantity)
        {
            M_Article article = GetArticle();
            IProviders providers = new Providers();

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

        public IProviders Satisfy(IDemandToProvidersMap demandToProvidersMap,
            IDbTransactionData dbTransactionData)
        {
            IProviders finalProviders = new Providers();
            Quantity remainingQuantity = GetQuantity();

            // satisfy by existing provider
            Provider providersByExisting =
                SatisfyByExistingNonExhaustedProvider(demandToProvidersMap, GetArticle());
            if (providersByExisting != null)
            {
                finalProviders.Add(providersByExisting);
                if (finalProviders.IsSatisfied(this))
                {
                    return finalProviders;
                }

                remainingQuantity =
                    finalProviders.GetMissingQuantity(this);
            }

            // satisfy by stock (includes productionOrder/PurchaseOrderPart)
            IProviders providersByStock = SatisfyByStock(remainingQuantity, dbTransactionData);
            finalProviders.AddAll(providersByStock);
            return finalProviders;
        }

        public Provider SatisfyByExistingNonExhaustedProvider(IDemandToProvidersMap demandToProvidersMap,
            M_Article article)
        {
            return demandToProvidersMap.FindNonExhaustedProvider(article);
        }

        public IProviders SatisfyByStock(Quantity missingQuantity,
            IDbTransactionData dbTransactionData)
        {
            IProviders providers = new Providers();

            // satisfy by stock
            Provider stockExchangeProvider = StockExchangeProvider.CreateStockProvider(GetArticle(), GetDueTime(),
                missingQuantity, _dbMasterDataCache, dbTransactionData);
            if (stockExchangeProvider != null)
            {
                providers.Add(stockExchangeProvider);
                missingQuantity = providers.GetMissingQuantity(this);
                if (missingQuantity.IsNull())
                {
                    return providers;
                }
            }
            
            // satisfy by provider
            Logger.Debug($"Satisfy by order for article {this}:");

            IProviders createdProviders = SatisfyByOrders(dbTransactionData, missingQuantity);
            // performance: is already called in SatisfyByOrders --> cache it
            Quantity providedQuantity = createdProviders.GetProvidedQuantity(this);
            providers.AddAll(createdProviders);
            // increase stock
            _dbMasterDataCache.M_StockGetByArticleId(GetArticleId()).Current +=
                providedQuantity.GetValue();
            
            return createdProviders;
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

        public NodeType GetNodeType()
        {
            return NodeType.Demand;
        }

        public INode GetEntity()
        {
            return this;
        }
    }
}