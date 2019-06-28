using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp;
using Zpp.DemandDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.ProviderDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Provider : IProviderLogic
    {
        protected readonly Guid _guid = Guid.NewGuid();
        protected readonly Demands _demands;
        protected readonly IProvider _provider;
        protected readonly IDbMasterDataCache _dbMasterDataCache;

        public Provider(IProvider provider, Demands demands, IDbMasterDataCache dbMasterDataCache)
        {
            if (provider == null)
            {
                throw new MrpRunException("Given provider should not be null.");
            }
            _demands = demands;
            _provider = provider;
            _dbMasterDataCache = dbMasterDataCache;
        }

        public Demands GetDemands()
        {
            return _demands;
        }
        
        protected DueTime GetDueTime()
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

            return _guid.Equals(item._guid);
        }
        
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public Quantity GetQuantity()
        {
            return _provider.GetQuantity();
        }

        public bool AnyDemands()
        {
            return _demands != null && _demands.Any();
        }

        public abstract Id GetArticleId();

        public bool ProvidesMoreThan(Quantity quantity)
        {
            return GetQuantity().IsGreaterThanOrEqualTo(quantity);
        }

        public T_Provider ToT_Provider()
        {
            return _provider.Provider;
        }
    }
}