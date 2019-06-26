using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.ProviderDomain;
using ZppForPrimitives;

namespace Zpp.DemandDomain
{
    /**
     * A wrapper for IDemand providing methods that every wrapped DemandType needs to implement
     */
    public interface IDemandLogic
    {
       
        Provider CreateProvider(IDbCache dbCache);

        // is needed to compare two instances
        DueTime GetDueTime();

        IDemand ToIDemand();

        bool HasProvider(Providers providers);
        
        Quantity GetQuantity();
    }
}