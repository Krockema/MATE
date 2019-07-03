using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.DemandDomain
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

        public override DueTime GetDueTime( )
        {
            DueTime dueTime = new DueTime(((T_StockExchange) _demand).RequiredOnTime);
            return dueTime;
        }

        public static Demand CreateStockExchangeProductionOrderDemand(M_ArticleBom articleBom, DueTime dueTime, IDbMasterDataCache dbMasterDataCache)
        {
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.Demand = new T_Demand();
            stockExchange.DemandId = stockExchange.Demand.Id;
            stockExchange.Quantity = articleBom.Quantity;
            stockExchange.State = State.Created;
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(new Id(articleBom.ArticleChildId));
            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Withdrawal;
            
            return new StockExchangeDemand(stockExchange, dbMasterDataCache);
        }
        
        public static Demand CreateStockExchangeStockDemand(M_Article article, DueTime dueTime, Quantity quantity, IDbMasterDataCache dbMasterDataCache)
        {
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.Demand = new T_Demand();
            stockExchange.DemandId = stockExchange.Demand.Id;
            stockExchange.Quantity = quantity.GetValue();
            stockExchange.State = State.Created;
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(article.GetId());
            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Insert;
            
            return new StockExchangeDemand(stockExchange, dbMasterDataCache);
        }
    }
}