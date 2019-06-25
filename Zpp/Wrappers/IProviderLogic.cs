using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    /**
     * A wrapper for IProvider providing methods that every wrapped ProviderType implements
     */
    public interface IProviderLogic
    {
        List<Demand> GetDemands();

        IProvider ToIProvider();
    }
}