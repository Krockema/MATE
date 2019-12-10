using System.Collections.Generic;

namespace Zpp.Mrp.MachineManagement
{
    /**
     * own Set impl, since HashSet ist not useful if:
     * - get any element e.g. first with O(1)
     * - pop any element with O(1)
     * - remove O(n), since list must be reindexed
     * - push() O(1)
     * but remains the idea of a mathematical set: every item exists only once
     */
    public interface IStackSet<T>: IEnumerable<T>
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

        int Count();
    }
}