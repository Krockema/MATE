using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp;
using Zpp.DemandDomain;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * A wrapper for IProvider providing methods that every wrapped ProviderType implements
     */
    public interface IProviderLogic
    {
        Demands GetDemands();

        IProvider ToIProvider();

        Quantity GetQuantity();

        bool AnyDemands();
    }
}