using System.Collections.Generic;

namespace Zpp.MachineDomain
{
    /**
     * own Set impl, since HashSet ist not useful if:
     * - get any element e.g. first with O(1)
     * - pop any element with O(1)
     * but remains the ide of a mathematical set: every item exists only once
     */
    public interface ISet<T>: IEnumerable<T>
    {
        void Add(T element);
        
        void AddAll(IEnumerable<T> elements);

        void Remove(T element);

        bool Any();

        T PopAny();

        T GetAny();

        int Count();
    }
}