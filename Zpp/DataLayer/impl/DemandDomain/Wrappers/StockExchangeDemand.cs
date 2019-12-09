using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Nominal;
using Zpp.Util;

namespace Zpp.DataLayer.impl.DemandDomain.Wrappers
{
    /**
     * wraps T_StockExchange for T_StockExchange demands
     */
    public class StockExchangeDemand : Demand, IDemandLogic
    {
        private readonly T_StockExchange _tStockExchangeDemand;

        public StockExchangeDemand(IDemand demand) : base(demand)
        {
            _tStockExchangeDemand = (T_StockExchange)demand;
        }

        public override IDemand ToIDemand()
        {
            return (T_StockExchange)_demand;
        }

        public override M_Article GetArticle( )
        {
            return _dbMasterDataCache.M_ArticleGetById(GetArticleId());
        }

        public override Id GetArticleId()
        {
            Id stockId = new Id(((T_StockExchange) _demand).StockId);
            M_Stock stock = _dbMasterDataCache.M_StockGetById(stockId);
            Id articleId = new Id(stock.ArticleForeignKey);
            return articleId;
        }

        public static Demand CreateStockExchangeProductionOrderDemand(M_ArticleBom articleBom, DueTime dueTime)
        {
            IDbMasterDataCache dbMasterDataCache =
                ZppConfiguration.CacheManager.GetMasterDataCache();
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
                new StockExchangeDemand(stockExchange);
            
            return stockExchangeDemand;
        }
        
        public static Demand CreateStockExchangeStockDemand(Id articleId, DueTime dueTime, Quantity quantity)
        {
            if (quantity == null || quantity.GetValue() == 0)
            {
                throw new MrpRunException("Quantity is not set.");
            }
            IDbMasterDataCache dbMasterDataCache =
                ZppConfiguration.CacheManager.GetMasterDataCache();
            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.StockExchangeType = StockExchangeType.Demand;
            stockExchange.Quantity = quantity.GetValue(); // TODO: PASCAL .GetValueOrDefault();
            stockExchange.State = State.Created;
            M_Stock stock = dbMasterDataCache.M_StockGetByArticleId(articleId);
            stockExchange.Stock = stock;
            stockExchange.StockId = stock.Id;
            stockExchange.RequiredOnTime = dueTime.GetValue();
            stockExchange.ExchangeType = ExchangeType.Insert;
            StockExchangeDemand stockExchangeDemand =
                new StockExchangeDemand(stockExchange);
            
            return stockExchangeDemand;
        }

        public bool IsTypeOfInsert()
        {
            return ((T_StockExchange) _demand).ExchangeType.Equals(ExchangeType.Insert);
        }
        
        public Id GetStockId()
        {
            return new Id(((T_StockExchange) _demand).StockId);
        }

        public override Duration GetDuration()
        {
            return Duration.Zero();
        }

        public override void SetStartTimeBackward(DueTime startTime)
        {
            if (_tStockExchangeDemand.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _tStockExchangeDemand.RequiredOnTime = startTime.GetValue();
        }
        
        public override void SetFinished()
        {
            _tStockExchangeDemand.State = State.Finished;
        }

        public override void SetInProgress()
        {
            if (_tStockExchangeDemand.State.Equals(State.Finished))
            {
                throw new MrpRunException("Impossible, the operation is already finished.");
            }
            _tStockExchangeDemand.State = State.InProgress;
        }

        public override DueTime GetEndTimeBackward()
        {
            return new DueTime(_tStockExchangeDemand.RequiredOnTime);
        }

        public override bool IsFinished()
        {
            return _tStockExchangeDemand.State.Equals(State.Finished);
        }

        public override void SetEndTimeBackward(DueTime endTime)
        {
            if (_tStockExchangeDemand.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _tStockExchangeDemand.RequiredOnTime = endTime.GetValue();
        }

        public override void ClearStartTimeBackward()
        {
            if (_tStockExchangeDemand.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _tStockExchangeDemand.RequiredOnTime = DueTime.INVALID_DUETIME;
        }

        public override void ClearEndTimeBackward()
        {
            if (_tStockExchangeDemand.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _tStockExchangeDemand.RequiredOnTime = DueTime.INVALID_DUETIME;
        }

        public override State? GetState()
        {
            return _tStockExchangeDemand.State;
        }
    }
}