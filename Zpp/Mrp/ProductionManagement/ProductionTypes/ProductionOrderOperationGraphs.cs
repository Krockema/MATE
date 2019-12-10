using System;
using System.Collections.Generic;
using System.Text;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.OrderGraph;

namespace Zpp.Mrp.ProductionManagement.ProductionTypes
{
    public class ProductionOrderOperationGraphs : Dictionary<ProductionOrder, IDirectedGraph<INode>>
    {
    }
}
