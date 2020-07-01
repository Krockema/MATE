using System.Collections;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.Util.Graph;
using Zpp.Util.StackSet;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    public class CollectionWrapperWithStackSet<T>: ICollectionWrapper<T> where T: class, IId
    {
        protected readonly IStackSet<T> StackSet = new StackSet<T>();

        protected CollectionWrapperWithStackSet()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return StackSet.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return StackSet.GetEnumerator();
        }

        public List<T> GetAll()
        {
            return StackSet.GetAll();
        }

        public virtual void Add(T item)
        {
            StackSet.Push(item);
        }

        public virtual void AddAll(IEnumerable<T> items)
        {
            StackSet.PushAll(items);
        }

        public T GetAny()
        {
            return StackSet.GetAny();
        }

        public IStackSet<T> ToStackSet()
        {
            return StackSet;
        }

        public virtual void Clear()
        {
            StackSet.Clear();
        }

        public virtual void Remove(T t)
        {
            StackSet.Remove(t);
        }

        public T GetById(Id id)
        {
            return StackSet.GetById(id);
        }

        public override string ToString()
        {
            return StackSet.ToString();
        }

        public bool Contains(T t)
        {
            return StackSet.Contains(t);
        }
    }
}