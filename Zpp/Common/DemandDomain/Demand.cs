using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DbCache;
using Zpp.OrderGraph;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain
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
        
        // TODO: use this method
        private int CalculatePriority(int dueTime, int operationDuration, int currentTime)
        {
            return dueTime - operationDuration - currentTime;
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

        public abstract DueTime GetStartTime(IDbTransactionData dbTransactionData);
    }
}