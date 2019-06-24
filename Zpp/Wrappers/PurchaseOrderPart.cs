using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class PurchaseOrderPart : Provider, IProviderLogic
    {
        public PurchaseOrderPart(IProvider provider, List<Demand> demands) : base(provider, demands)
        {
        }
    }
}