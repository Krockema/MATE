using System.Collections.Generic;

namespace Zpp.Wrappers
{
    /**
     * A wrapper for IProvider providing methods that every wrapped ProviderType implements
     */
    public interface IProviderLogic
    {
        List<Demand> GetDemands();
    }
}