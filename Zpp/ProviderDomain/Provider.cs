using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;
using ZppForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Provider : IProviderLogic
    {
        protected readonly Guid _guid = new Guid();
        protected readonly List<Demand> _demands;
        protected readonly IProvider _provider;

        public Provider(IProvider provider, List<Demand> demands)
        {
            _demands = demands;
            _provider = provider;
        }

        public Provider()
        {
            
        }

        public List<Demand> GetDemands()
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
    }
}