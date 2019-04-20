
using System.Collections.Generic;
using Zpp.Utils;

namespace Zpp.Utils
{
    public interface ITree<T>
    {
        List<T> getChildNodes(T node);
        T getRootNode();
    }
}