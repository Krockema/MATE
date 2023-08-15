using System;

namespace Mate.DataCore.Interfaces
{
    public interface IOperation
    {
        int HierarchyNumber { get; set; }
        string Name { get; set; }
        TimeSpan Duration { get; set; }
        int ResourceCapabilityId { get; set; }
    }
}
