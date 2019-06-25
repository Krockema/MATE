using System.Collections.Generic;
using Master40.DB.Interfaces;
using Zpp.DemandDomain;

namespace Zpp.ProviderDomain
{
    /**
     * A wrapper for IProvider providing methods that every wrapped ProviderType implements
     */
    public interface IProviderLogic
    {
        List<Demand> GetDemands();

        IProvider ToIProvider();

        bool isFor(Demand demand);
    }
}