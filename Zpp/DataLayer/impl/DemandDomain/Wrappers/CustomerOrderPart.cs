using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Nominal;
using Zpp.Util;

namespace Zpp.DataLayer.impl.DemandDomain.Wrappers
{
    public class CustomerOrderPart : Demand
    {
        private T_CustomerOrderPart _customerOrderPart;
        
        public CustomerOrderPart(IDemand demand) : base(demand)
        {
            _customerOrderPart = (T_CustomerOrderPart) demand;
        }

        public override IDemand ToIDemand()
        {
            return (T_CustomerOrderPart)_demand;
        }

        public override M_Article GetArticle()
        {
            return _dbMasterDataCache.M_ArticleGetById(GetArticleId());
        }

        public override Id GetArticleId()
        {
            return new Id(_customerOrderPart.ArticleId);
        }

        public T_CustomerOrderPart GetValue()
        {
            return (T_CustomerOrderPart)_demand;
        }

        public override Duration GetDuration()
        {
            return Duration.Zero();
        }

        public override void SetStartTimeBackward(DueTime startTime)
        {
            // is NOT allowed to change
            throw new System.NotImplementedException();
        }

        public override void SetFinished()
        {
            _customerOrderPart.State = State.Finished;
        }

        public override void SetInProgress()
        {
            if (_customerOrderPart.State.Equals(State.Finished))
            {
                throw new MrpRunException("Impossible, the operation is already finished.");
            }
            _customerOrderPart.State = State.InProgress;
        }

        private void EnsureCustomerOrderIsLoaded()
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            Id customerOrderId = new Id(_customerOrderPart.CustomerOrderId);
            _customerOrderPart.CustomerOrder =
                dbTransactionData.CustomerOrderGetById(customerOrderId);
            if (_customerOrderPart.CustomerOrder == null)
            {
                throw new MrpRunException($"A CustomerOrderPart{this} cannot exists without an CustomerOrder.");
            }
        }

        public override DueTime GetEndTimeBackward()
        {
            EnsureCustomerOrderIsLoaded();
            DueTime dueTime = new DueTime(_customerOrderPart.CustomerOrder.DueTime);
            return dueTime;
        }

        public override bool IsFinished()
        {
            return _customerOrderPart.State.Equals(State.Finished);
        }

        public override void SetEndTimeBackward(DueTime endTime)
        {
            throw new System.NotImplementedException();
        }

        public override void ClearStartTimeBackward()
        {
            throw new System.NotImplementedException();
        }

        public override void ClearEndTimeBackward()
        {
            throw new System.NotImplementedException();
        }

        public override State? GetState()
        {
            return _customerOrderPart.State;
        }

        public Id GetCustomerOrderId()
        {
            return new Id(_customerOrderPart.CustomerOrderId);
        }
    }
}