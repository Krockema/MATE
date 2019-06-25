using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    /**
     * wraps T_PurchaseOrderPart
     */
    public class PurchaseOrderPart : Provider, IProviderLogic
    {
        public PurchaseOrderPart(IProvider provider, List<Demand> demands) : base(provider, demands)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_PurchaseOrderPart)_provider;
        }
    }
}