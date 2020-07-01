using System;
using System.Collections.Generic;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.Util.Graph.impl
{
    public sealed class Nodes : CollectionWrapperWithStackSet<INode>, INodes
    {
        public Nodes(IEnumerable<INode> list)
        {
            AddAll(list);
        }

        public Nodes(INode item)
        {
            Add(item);
        }

        public Nodes()
        {
        }

        public IEnumerable<T> As<T>()
        {
            List<T> newList = new List<T>();
            foreach (var item in StackSet)
            {
                newList.Add((T)item);
            }

            return newList;
        }

        public Stack<INode> ToStack()
        {
            Stack<INode> stack = new Stack<INode>();
            foreach (var item in StackSet)
            {
                stack.Push(item);
            }

            return stack;
        }
    }
}