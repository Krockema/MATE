using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;

namespace Zpp.Wrappers
{
    public class WT_PurchaseOrderPart : Provider, WIProvider
    {
        public WT_PurchaseOrderPart(IProvider provider, List<WIDemand> demands) : base(provider, demands)
        {
        }
    }
}