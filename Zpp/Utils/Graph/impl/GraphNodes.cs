using System.Collections.Generic;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.Util.Graph.impl
{
    public sealed class GraphNodes: CollectionWrapperWithStackSet<IGraphNode>
    {
        internal GraphNodes(IEnumerable<IGraphNode> list)
        {
            AddAll(list);
        }

        internal GraphNodes()
        {
        }
    }
}