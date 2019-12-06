using System.Collections.Generic;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.Util.Graph
{
    public interface INodes : ICollectionWrapper<INode>
    {
        IEnumerable<T> As<T>();

        Stack<INode> ToStack();
    }
}