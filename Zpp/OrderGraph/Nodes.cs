using System.Collections;
using System.Collections.Generic;

namespace Zpp
{
    public class Nodes : CollectionWrapperWithList<INode>, INodes
    {
        public Nodes(List<INode> list) : base(list)
        {
        }

        public Nodes()
        {
        }
    }
}