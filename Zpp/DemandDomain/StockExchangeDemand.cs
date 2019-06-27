using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.DemandDomain
{
    /**
     * wraps T_StockExchange for T_StockExchange demands
     */
    public class StockExchangeDemand : Demand, IDemandLogic
    {

        public StockExchangeDemand(IDemand demand, IDbCacheMasterData dbCacheMasterData) : base(demand, dbCacheMasterData)
        {
        }

        public override IDemand ToIDemand()
        {
            return (T_StockExchange)_demand;
        }

        public override M_Article GetArticle( )
        {
            Id stockId = new Id(((T_StockExchange) _demand).StockId);
            M_Stock stock = _dbCacheMasterData.M_StockGetById(stockId);
            Id articleId = stock.GetId();
            return _dbCacheMasterData.M_ArticleGetById(articleId);
        }

        public override DueTime GetDueTime( )
        {
            DueTime dueTime = new DueTime(((T_StockExchange) _demand).RequiredOnTime);
            return dueTime;
        }
    }
}