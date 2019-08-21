using Master40.DB.Data.WrappersForPrimitives;
using Xunit;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;

namespace Zpp
{
    public class ProductionOrderDirectedGraph : DemandToProviderDirectedGraph, IDirectedGraph<INode>
    {
        public ProductionOrderDirectedGraph(IDbTransactionData dbTransactionData) : base(
            dbTransactionData)
        {
            foreach (var uniqueNode in GetAllUniqueNode())
            {
                if (uniqueNode.GetEntity().GetType() != typeof(ProductionOrder) 
                    // && uniqueNode.GetEntity().GetType() != typeof(ProductionOrderBom)
                    )
                {
                    RemoveNode(uniqueNode);
                }

                /*if (uniqueNode.GetEntity().GetType() == typeof(ProductionOrderBom))
                {
                    ProductionOrderBom productionOrderBom = (ProductionOrderBom)uniqueNode.GetEntity();
                    if (productionOrderBom.HasOperation() == false)
                    {
                        RemoveNode(uniqueNode);
                    }
                }*/
            }
        }
    }
}