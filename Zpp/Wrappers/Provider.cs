using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    /**
     * Provides default implementations for interface methods, can be moved to interface once C# 8.0 is released
     */
    public abstract class Provider : WIProvider
    {
        protected List<WIDemand> Demands;
        protected IProvider _provider;

        public Provider(IProvider provider, List<WIDemand> demands)
        {
            Demands = demands;
            _provider = provider;
        }

        public Provider()
        {
            
        }

        public List<WIDemand> GetDemands()
        {
            return Demands;
        }
    }
}