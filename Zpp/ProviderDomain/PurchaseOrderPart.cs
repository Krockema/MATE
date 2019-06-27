using System.Collections.Generic;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_PurchaseOrderPart
     */
    public class PurchaseOrderPart : Provider, IProviderLogic
    {
        public PurchaseOrderPart(IProvider provider, Demands demands) : base(provider, demands)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_PurchaseOrderPart)_provider;
        }
    }
}