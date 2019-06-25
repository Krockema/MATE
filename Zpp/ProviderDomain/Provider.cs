using System;
using System.Collections.Generic;
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
        protected List<Demand> _demands;
        protected IProvider _provider;

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

        public bool isFor(Demand demand)
        {
            foreach (var loopDemand in _demands)
            {
                throw new NotImplementedException();
            }
        }
    }
}