using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Nominal;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.Util;
using Zpp.Util.Graph.impl;

namespace Zpp.DataLayer.impl.DemandDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Demand : IDemandLogic, IDemandOrProvider
    {
        
        protected readonly IDemand _demand;
        protected readonly IDbMasterDataCache _dbMasterDataCache = ZppConfiguration.CacheManager.GetMasterDataCache();
        private readonly Id _id;

        public Demand(IDemand demand)
        {
            if (demand == null)
            {
                throw new MrpRunException("Given demand should not be null.");
            }

            _id = new Id(demand.Id);
            _demand = demand;
            
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
            string state = Enum.GetName(typeof(State), GetState());
            return $"{GetId()}: {GetArticle().Name}; {GetQuantity()}; {state}; IsReadOnly: {IsReadOnly()}";
        }

        public bool IsReadOnly()
        {
            return _demand.IsReadOnly;
        }

        public abstract M_Article GetArticle();

        public Id GetId()
        {
            return _id;
        }

        public abstract Id GetArticleId();
        
        public NodeType GetNodeType()
        {
            return NodeType.Demand;
        }

        public DueTime GetStartTimeBackward()
        {
            if (GetEndTimeBackward().IsInvalid())
            {
                return null;
            }
            return GetEndTimeBackward().Minus(new DueTime(GetDuration()));
        }

        public abstract DueTime GetEndTimeBackward();

        public abstract Duration GetDuration();

        public abstract void SetStartTimeBackward(DueTime startTime);

        public static Demand AsDemand(IDemandOrProvider demandOrProvider)
        {
            if (demandOrProvider.GetType() == typeof(ProductionOrderBom))
            {
                return (ProductionOrderBom)demandOrProvider;
            }
            else if (demandOrProvider.GetType() == typeof(StockExchangeDemand))
            {
                return (StockExchangeDemand)demandOrProvider;
            }
            else if (demandOrProvider.GetType() == typeof(CustomerOrderPart))
            {
                return (CustomerOrderPart)demandOrProvider;
            }
            else
            {
                throw new MrpRunException("Unknown type implementing Demand");
            }
        }

        public abstract void SetFinished();

        public abstract void SetInProgress();

        public abstract bool IsFinished();
        
        public  abstract void SetEndTimeBackward(DueTime endTime);

        public abstract void ClearStartTimeBackward();

        public abstract void ClearEndTimeBackward();

        public abstract State? GetState();

        public void SetReadOnly()
        {
            _demand.IsReadOnly = true;
        }
    }
}