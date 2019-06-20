using Master40.DB.Interfaces;
using Zpp.Entities;

namespace Zpp.Wrappers
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface WIDemand
    {
        WIProvider CreateProvider(IDbCache dbCache);
    }
}