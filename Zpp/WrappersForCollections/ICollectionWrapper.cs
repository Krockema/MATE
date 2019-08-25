using System.Collections.Generic;

namespace Zpp
{
    public interface ICollectionWrapper<T>: IEnumerable<T>
    {
        List<T> GetAll();

        void Add(T item);
        
        void AddAll(IEnumerable<T> items);
    }
}