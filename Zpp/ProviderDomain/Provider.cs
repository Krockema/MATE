using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.DemandDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.LotSize;

namespace Zpp.ProviderDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Provider : IProviderLogic, INode
    {
        protected Demands _dependingDemands;
        protected readonly IProvider _provider;
        protected readonly DemandToProviderTable _demandToProviderTable = new DemandToProviderTable();
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
        
        public DueTime GetDueTime()
        {
            return new DueTime(_provider.GetDueTime());
        }

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

        public abstract void CreateNeededDemands(M_Article article,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Provider parentProvider, Quantity quantity);
        
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

        public abstract string GetGraphizString();
    }
}