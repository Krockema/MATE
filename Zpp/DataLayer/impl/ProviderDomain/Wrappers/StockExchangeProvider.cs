using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.DB.Interfaces;
using Zpp.Util;

namespace Zpp.DataLayer.impl.ProviderDomain.Wrappers
{
    /**
     * wraps T_StockExchange for T_StockExchange providers
     */
    public class StockExchangeProvider : Provider, IProviderLogic
    {
        private readonly T_StockExchange _stockExchangeProvider;
        public StockExchangeProvider(IProvider provider) :
            base(provider)
        {
            _stockExchangeProvider = (T_StockExchange) provider;
        }

        public override IProvider ToIProvider()
        {
            return (T_StockExchange) _provider;
        }

        public override Id GetArticleId()
        {
            Id stockId = new Id(((T_StockExchange) _provider).StockId);
            M_Stock stock = _dbMasterDataCache.M_StockGetById(stockId);
            return new Id(stock.ArticleForeignKey);
        }

        public Id GetStockId()
        {
            return new Id(((T_StockExchange) _provider).StockId);
        }

        public override void SetProvided(DueTime atTime)
        {
            if (_stockExchangeProvider.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _stockExchangeProvider.State = State.Finished;
            _stockExchangeProvider.Time = atTime.GetValue();
        }

        public override void SetStartTimeBackward(DueTime startTime)
        {
            if (_stockExchangeProvider.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _stockExchangeProvider.RequiredOnTime = startTime.GetValue();
        }
        
        public override void SetFinished()
        {
            _stockExchangeProvider.State = State.Finished;
        }

        public override void SetInProgress()
        {
            if (_stockExchangeProvider.State.Equals(State.Finished))
            {
                throw new MrpRunException("Impossible, the operation is already finished.");
            }
            _stockExchangeProvider.State = State.InProgress;
        }
        
        public override Duration GetDuration()
        {
            return Duration.Zero();
        }

        public override DueTime GetEndTimeBackward()
        {
            return new DueTime(_stockExchangeProvider.RequiredOnTime);
        }

        public override bool IsFinished()
        {
            return _stockExchangeProvider.State.Equals(State.Finished);
        }

        public override void SetEndTimeBackward(DueTime endTime)
        {
            if (_stockExchangeProvider.IsReadOnly)
            {
                throw new MrpRunException("A readOnly entity cannot be changed anymore.");
            }
            _stockExchangeProvider.RequiredOnTime = endTime.GetValue();
        }

        public override void ClearStartTimeBackward()
        {
            _stockExchangeProvider.RequiredOnTime = DueTime.INVALID_DUETIME;
        }

        public override void ClearEndTimeBackward()
        {
            _stockExchangeProvider.RequiredOnTime = DueTime.INVALID_DUETIME;
        }

        public override State? GetState()
        {
            return _stockExchangeProvider.State;
        }
    }
}