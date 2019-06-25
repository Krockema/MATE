using System.Collections.Generic;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.Wrappers
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Provider : IProviderLogic
    {
        protected List<Demand> Demands;
        protected IProvider _provider;

        public Provider(IProvider provider, List<Demand> demands)
        {
            Demands = demands;
            _provider = provider;
        }

        public Provider()
        {
            
        }

        public List<Demand> GetDemands()
        {
            return Demands;
        }
        
        protected DueTime GetDueTime()
        {
            return new DueTime(_provider.GetDueTime());
        }

        public abstract IProvider ToIProvider();
    }
}