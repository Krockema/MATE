using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.DbCache;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain.Wrappers
{
    /**
     * wraps T_StockExchange for T_StockExchange demands
     */
    public class StockExchangeDemand : Demand, IDemandLogic
    {

        public StockExchangeDemand(IDemand demand, IDbMasterDataCache dbMasterDataCache) : base(demand, dbMasterDataCache)
        {
        }

        public override IDemand ToIDemand()
        {
            return (T_StockExchange)_demand;
        }

        public override M_Article GetArticle( )
        {
            Id stockId = new Id(((T_StockExchange) _demand).StockId);
            M_Stock stock = _dbMasterDataCache.M_StockGetById(stockId);
            Id articleId = new Id(stock.ArticleForeignKey);
            return _dbMasterDataCache.M_ArticleGetById(articleId);
        }

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            DueTime dueTime = new DueTime(((T_StockExchange) _demand).RequiredOnTime);
            return dueTime;
        }

        public static Demand CreateStockExchangeProductionOrderDemand(M_ArticleBom articleBom, DueTime dueTime, IDbMasterDataCache dbMasterDataCache)
        {
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.StockExchangeType = StockExchangeType.Demand;
            stockExchange.Quantity = articleBom.Quantity;
            stockExchange.State = State.Created;
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(new Id(articleBom.ArticleChildId));
            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Withdrawal;
            
            StockExchangeDemand stockExchangeDemand =
                new StockExchangeDemand(stockExchange, dbMasterDataCache);
            
            return stockExchangeDemand;
        }
        
        public static Demand CreateStockExchangeStockDemand(M_Article article, DueTime dueTime, Quantity quantity, IDbMasterDataCache dbMasterDataCache)
        {
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.StockExchangeType = StockExchangeType.Demand;
            stockExchange.Quantity = quantity.GetValue();
            stockExchange.State = State.Created;
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(article.GetId());
            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Insert;
            StockExchangeDemand stockExchangeDemand =
                new StockExchangeDemand(stockExchange, dbMasterDataCache);
            
            return stockExchangeDemand;
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string exchangeType = Constants.EnumToString(((T_StockExchange)_demand).ExchangeType, typeof(ExchangeType));
            string graphizString = $"D(SE:{exchangeType[0]});{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public bool IsTypeOfInsert()
        {
            return ((T_StockExchange) _demand).ExchangeType.Equals(ExchangeType.Insert);
        }
        
        public Id GetStockId()
        {
            return new Id(((T_StockExchange) _demand).StockId);
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            return GetDueTime(dbTransactionData);
        }
    }
}