using Master40.DB.Data.WrappersForPrimitives;
using Zpp.ProviderDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

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

        public Quantity SatisfyByOrders(IDbTransactionData dbTransactionData,
            Quantity demandedQuantity, IProviderManager providerManager, Demand demand)
        {
            Logger.Debug($"Satisfy by order for article {this}:");
            Quantity remainingQuantity = new Quantity(demandedQuantity);

            // make or buy
            if (demand.GetArticle().ToBuild)
            {
                LotSize.LotSize lotSizes = new LotSize.LotSize(demandedQuantity, GetArticleId());
                foreach (var lotSize in lotSizes.GetLotSizes())
                {
                    ProductionOrder productionOrder =
                        ProductionOrder.CreateProductionOrder(this, dbTransactionData,
                            _dbMasterDataCache, lotSize);
                    Logger.Debug("ProductionOrder created.");
                     remainingQuantity = providerManager.AddProvider(this,
                        productionOrder);
                }

                return remainingQuantity;
            }

            // TODO: revisit all methods that have this as parameter
            // TODO: here is no lotSize used !
            remainingQuantity = PurchaseOrderPart.CreatePurchaseOrderPart(this, GetArticle(),
                GetDueTime(dbTransactionData), demandedQuantity, _dbMasterDataCache, providerManager);
            Logger.Debug("PurchaseOrderPart created.");

            return remainingQuantity;
        }

        // TODO: use this method
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
        }

        public abstract IDemand GetIDemand();

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
            return $"{GetId()}: {GetArticle().Name};{GetQuantity()}";
        }

        public abstract M_Article GetArticle();

        public abstract DueTime GetDueTime(IDbTransactionData dbTransactionData);

        public Id GetId()
        {
            return new Id(_demand.Id);
        }

        public Id GetArticleId()
        {
            return GetArticle().GetId();
        }

        public void SatisfyStockExchangeDemand(IProviderManager providerManager, IDbTransactionData dbTransactionData)
        {
            Quantity remainingQuantity = GetQuantity();

            // satisfy by existing provider
            remainingQuantity = SatisfyByExistingNonExhaustedProvider(providerManager, this, remainingQuantity);
            if (remainingQuantity.IsNull())
            {
                return;
            }

            // satisfy by order
            remainingQuantity = SatisfyByOrders(dbTransactionData, remainingQuantity,
                providerManager, this);

            if (remainingQuantity.IsGreaterThan(Quantity.Null()))
            {
                throw new MrpRunException($"The demand({this}) was NOT satisfied.");
            }
        }

        public Quantity SatisfyByExistingNonExhaustedProvider(IProviderManager providerManager,
            Demand demand, Quantity remainingQuantity)
        {
            return providerManager.ReserveQuantityOfExistingProvider(demand.GetId(), demand.GetArticle(),
                remainingQuantity);
        }

        public Quantity SatisfyByStock(Quantity missingQuantity,
            IDbTransactionData dbTransactionData, IProviderManager providerManager, Demand demand)
        {
            Quantity remainingQuantity = missingQuantity;

            // satisfy by existing provider
            remainingQuantity = SatisfyByExistingNonExhaustedProvider(providerManager, this, remainingQuantity);
            if (remainingQuantity.IsNull())
            {
                return remainingQuantity;
            }
            
            // satisfy by stock
            Provider stockExchangeProvider = StockExchangeProvider.CreateStockExchangeProvider(GetArticle(),
                GetDueTime(dbTransactionData), remainingQuantity, _dbMasterDataCache, dbTransactionData);
            remainingQuantity.DecrementBy(stockExchangeProvider.GetQuantity());
            if (stockExchangeProvider == null)
            {
                throw new MrpRunException("No stockExchangeProvider was created."); 
            }

            providerManager.AddProvider(demand, stockExchangeProvider);
            return remainingQuantity;
        }

        public NodeType GetNodeType()
        {
            return NodeType.Demand;
        }

        public INode GetEntity()
        {
            return this;
        }

        public virtual string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            return $"{GetQuantity()};\\n{GetArticle().Name};{GetDueTime(dbTransactionData)}";
        }

        public string GetJsonString(IDbTransactionData dbTransactionData)
        {
            throw new System.NotImplementedException();
        }
    }
}