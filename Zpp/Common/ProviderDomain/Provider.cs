using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.OrderGraph;
using Zpp.Utils;
using Zpp.WrappersForCollections;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.ProviderDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Provider : IProviderLogic, INode
    {
        protected Demands _dependingDemands;
        protected readonly ProviderToDemandTable ProviderToDemandTable = new ProviderToDemandTable();
        protected readonly IProvider _provider;
        protected readonly IDbMasterDataCache _dbMasterDataCache;
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Provider(IProvider provider, IDbMasterDataCache dbMasterDataCache)
        {
            if (provider == null)
            {
                throw new MrpRunException("Given provider should not be null.");
            }
            _provider = provider;
            _dbMasterDataCache = dbMasterDataCache;
        }

        public Demands GetAllDependingDemands()
        {
            return _dependingDemands;
        }

        public abstract DueTime GetDueTime(IDbTransactionData dbTransactionData);

        public abstract IProvider ToIProvider();
        
        public override bool Equals(object obj)
        {
            var item = obj as Provider;

            if (item == null)
            {
                return false;
            }

            return _provider.Id.Equals(item._provider.Id);
        }
        
        public override int GetHashCode()
        {
            return _provider.Id.GetHashCode();
        }

        public Quantity GetQuantity()
        {
            return _provider.GetQuantity();
        }

        public bool AnyDependingDemands()
        {
            return _dependingDemands != null && _dependingDemands.Any();
        }

        public abstract Id GetArticleId();

        public bool ProvidesMoreThan(Quantity quantity)
        {
            return GetQuantity().IsGreaterThanOrEqualTo(quantity);
        }

        public abstract void CreateDependingDemands(M_Article article,
            IDbTransactionData dbTransactionData, Provider parentProvider, Quantity demandedQuantity);
        
        /**
         * returns Quantity.Null or higher
         */
        public static Quantity CalcQuantityProvidedByProvider(Quantity providedQuantity,
            Quantity demandedQuantity)
        {
            if (!providedQuantity.IsGreaterThan(Quantity.Null()))
            {
                return Quantity.Null();
            }

            if (providedQuantity.IsGreaterThanOrEqualTo(demandedQuantity))
            {
                return demandedQuantity;
            }

            return providedQuantity;
        }

        public M_Article GetArticle()
        {
            return _dbMasterDataCache.M_ArticleGetById(GetArticleId());
        }

        public Id GetId()
        {
            return new Id(_provider.Id);
        }

        public override string ToString()
        {
            return $"{GetId()}: {GetArticle().Name};{GetQuantity()}";
        }

        public NodeType GetNodeType()
        {
            return NodeType.Provider;
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
        
        /**
         * Adapts the dueTime and also adapts the startTime accordingly (if exists)
         */
        public abstract void SetDueTime(DueTime newDueTime, IDbTransactionData dbTransactionData);

        public ProviderToDemandTable GetProviderToDemandTable()
        {
            return ProviderToDemandTable;
        }

        public void AddProviderToDemand(T_ProviderToDemand providerToDemand)
        {
            ProviderToDemandTable.Add(providerToDemand);
        }
    }
}