using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.ProviderDomain;

namespace Zpp.StockDomain
{
    public class StockManager
    {
        private readonly Dictionary<Id, Stock> _stocks = new Dictionary<Id, Stock>();
        private readonly HashSet<Provider> _alreadyConsideredProviders = new HashSet<Provider>();

        public StockManager(StockManager stockManager)
        {
            foreach (var stock in stockManager.GetStocks())
            {
                _stocks.Add(stock.GetArticleId(),
                    new Stock(stock.GetQuantity(), stock.GetArticleId(), stock.GetMinStockLevel()));
            }
        }

        public StockManager(List<M_Stock> stocks)
        {
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
            if (provider.GetType() == typeof(StockExchangeProvider))
            {
                Stock stock = _stocks[provider.GetArticleId()];
                stock.DecrementBy(provider.GetQuantity());
                Quantity currentQuantity = stock.GetQuantity();
                if (currentQuantity.IsSmallerThan(stock.GetMinStockLevel()))
                {
                    provider.CreateNeededDemands(provider.GetArticle(), dbTransactionData,
                        provider, stock.GetMinStockLevel().Minus(currentQuantity));
                }
            }
            // PrO, PuOP increases stock
            else
            {
                _stocks[provider.GetArticleId()].IncrementBy(provider.GetQuantity());
            }
        }

        public void AdaptStock(StockManager stockManager)
        {
            foreach (var stock in stockManager.GetStocks())
            {
                _stocks[stock.GetArticleId()].IncrementBy(stock.GetQuantity());
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
    }
}