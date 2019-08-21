using System.Collections;
using System.Collections.Generic;

namespace Zpp
{
    public class CollectionWrapperWithList<T>: ICollectionWrapper<T>
    {
        protected readonly List<T> List = new List<T>();

        /**
         * Init collectionWrapper with a copy of given list
         */
        protected CollectionWrapperWithList(List<T> list)
        {
            foreach (var item in list)
            {
                List.Add(item);
            }
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
    }
}