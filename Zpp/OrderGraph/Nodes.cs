using System.Collections.Generic;
using Zpp.WrappersForCollections;

namespace Zpp.OrderGraph
{
    public class Nodes : CollectionWrapperWithList<INode>, INodes
    {
        public Nodes(List<INode> list) : base(list)
        {
        }

        public Nodes(INode item) : base(item)
        {
        }

        public Nodes()
        {
        }
    }
}