using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Zpp.Util.Graph;

namespace Zpp.Util.StackSet
{
    /**
     * own Set impl, since HashSet ist not useful if:
     * - get any element e.g. first with O(1)
     * - pop any element with O(1)
     * - remove O(n), since list must be reindexed
     * - push() O(1)
     * - GetById(), Contains(): O(1)
     * but remains the idea of a mathematical set: every item exists only once
     */
    public interface IStackSet<T>: IEnumerable<T> where T: IId
    {
        void Push(T element);
        
        void PushAll(IEnumerable<T> elements);

        void Remove(T element);

        bool Any();

        T PopAny();

        T GetAny();

        /**
         * Should return a new list which contains all elements
         */
        List<T> GetAll();
        
        List<T2> GetAllAs<T2>()  where T2: T;

        void Clear();

        IStackSet<T2> As<T2>() where T2: T;

        int Count();

        T GetById(Id id);

        bool Contains(T t);
        
        bool Contains(Id id);

        void RemoveById(Id id);
    }
}