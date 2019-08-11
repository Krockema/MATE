using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.WrappersForPrimitives;

namespace Zpp.StockDomain
{
    public class StockManager
    {
        private readonly Dictionary<Id, Stock> _stocks = new Dictionary<Id, Stock>();
        private HashSet<Provider> _alreadyConsideredProviders = new HashSet<Provider>();
        private readonly IDbMasterDataCache _dbMasterDataCache;

        // for duplicating it
        public StockManager(StockManager stockManager, IDbMasterDataCache dbMasterDataCache)
        {
            _dbMasterDataCache = dbMasterDataCache;
            foreach (var stock in stockManager.GetStocks())
            {
                _stocks.Add(stock.GetArticleId(),
                    new Stock(stock.GetQuantity(), stock.GetArticleId(), stock.GetMinStockLevel()));
            }
        }

        public StockManager(List<M_Stock> stocks, IDbMasterDataCache dbMasterDataCache)
        {
            _dbMasterDataCache = dbMasterDataCache;
            foreach (var stock in stocks)
            {
                Id articleId = new Id(stock.ArticleForeignKey);
                Stock myStock = new Stock(new Quantity(stock.Current), articleId,
                    new Quantity(stock.Min));
                _stocks.Add(articleId, myStock);
            }
        }

        public void AdaptStock(Provider provider, IDbTransactionData dbTransactionData)
        {
            // a provider can influence the stock only once
            if (_alreadyConsideredProviders.Contains(provider))
            {
                return;
            }

            _alreadyConsideredProviders.Add(provider);

            // SE:W decrements stock
            Stock stock = _stocks[provider.GetArticleId()];
            if (provider.GetType() == typeof(StockExchangeProvider))
            {
                stock.DecrementBy(provider.GetQuantity());
                Quantity currentQuantity = stock.GetQuantity();
                if (currentQuantity.IsSmallerThan(stock.GetMinStockLevel()))
                {
                    provider.CreateDependingDemands(provider.GetArticle(), dbTransactionData, provider,
                        stock.GetMinStockLevel().Minus(currentQuantity));
                }
            }
            // PrO, PuOP increases stock
            else
            {
                stock.IncrementBy(provider.GetQuantity());
            }
        }

        /**
         * overrides (copy) stocks (including quantity) from given stockManager to this
         */
        public void AdaptStock(StockManager stockManager)
        {
            foreach (var stock in stockManager.GetStocks())
            {
                _stocks[stock.GetArticleId()] = stock;
                _alreadyConsideredProviders = stockManager._alreadyConsideredProviders;
            }
        }

        public List<Stock> GetStocks()
        {
            return _stocks.Values.ToList();
        }

        public static void CalculateCurrent(M_Stock stock, IDbTransactionData dbTransactionData,
            Quantity startQuantity)
        {
            Quantity currentQuantity = new Quantity(startQuantity);
            // TODO
        }

        public Stock GetStockById(Id id)
        {
            return _stocks[id];
        }

        public Quantity Satisfy(Quantity demandedQuantity, IDbTransactionData dbTransactionData,
            IProviderManager providerManager, Demand demand)
        {
            Stock stock = _stocks[demand.GetArticleId()];
            if (stock.GetQuantity().IsGreaterThan(Quantity.Null()))
            {
                Quantity reservedQuantity;
                if (demandedQuantity.IsGreaterThan(stock.GetQuantity()))
                {
                    reservedQuantity = stock.GetQuantity();
                }
                else
                {
                    reservedQuantity = demandedQuantity;
                }
                    
                Provider stockProvider = CreateStockExchangeProvider(demand.GetArticle(),
                    demand.GetDueTime(dbTransactionData), reservedQuantity, _dbMasterDataCache,
                    dbTransactionData);
                // this implicitly adapts the stock, createNeededDemands
                providerManager.AddProvider(demand, stockProvider, reservedQuantity);
                return demandedQuantity.Minus(stockProvider.GetQuantity());
            }
            else
            {
                return demandedQuantity;
            }
        }

        /**
         * returns a provider, which can be a stockExchangeProvider, if article can be fulfilled by stock, else
         * a productionOrder/purchaseOrderPart
         */
        public Provider CreateStockExchangeProvider(M_Article article, DueTime dueTime,
            Quantity demandedQuantity, IDbMasterDataCache dbMasterDataCache,
            IDbTransactionData dbTransactionData)
        {
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(article.GetId());
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.StockExchangeType = StockExchangeType.Provider;
            stockExchange.Quantity = demandedQuantity.GetValue();
            stockExchange.State = State.Created;

            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Withdrawal;
            StockExchangeProvider stockExchangeProvider =
                new StockExchangeProvider(stockExchange, dbMasterDataCache);

            return stockExchangeProvider;
        }
    }
}