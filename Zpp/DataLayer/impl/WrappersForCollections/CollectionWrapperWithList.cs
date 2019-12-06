using System.Collections;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Util.Graph;
using Zpp.Util.StackSet;

namespace Zpp.DataLayer.impl.WrappersForCollections
{
    public class CollectionWrapperWithList<T>: ICollectionWrapper<T> where T: IScheduleNode
    {
        protected readonly List<T> List = new List<T>();

        /**
         * Init collectionWrapper with a copy of given list
         */
        protected CollectionWrapperWithList(IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                List.Add(item);
            }
        }
        
        protected CollectionWrapperWithList(T item)
        {
            List.Add(item);
        }

        protected CollectionWrapperWithList()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public List<T> GetAll()
        {
            return List;
        }

        public void Add(T item)
        {
            List.Add(item);
        }

        public void AddAll(IEnumerable<T> items)
        {
            List.AddRange(items);
        }

        public T GetAny()
        {
            return List[0];
        }

        public IStackSet<T> ToStackSet()
        {
            return new StackSet<T>(List);
        }

        public void Clear()
        {
            List.Clear();
        }

        public void Remove(T t)
        {
            List.Remove(t);
        }

        public T GetById(Id id)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(T t)
        {
            throw new System.NotImplementedException();
        }
    }
}