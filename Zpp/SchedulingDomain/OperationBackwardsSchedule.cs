using Zpp.WrappersForPrimitives;

namespace Zpp.SchedulingDomain
{
    public class OperationBackwardsSchedule
    {
        public DueTime EndBackwards { get; set; }
        public DueTime StartBackwards { get; set; }
        public HierarchyNumber HierarchyNumber { get; set; }

        public OperationBackwardsSchedule()
        {
        }

        public OperationBackwardsSchedule(DueTime endBackwards, DueTime startBackwards, HierarchyNumber hierarchyNumber)
        {
            EndBackwards = endBackwards;
            StartBackwards = startBackwards;
            HierarchyNumber = hierarchyNumber;
        }
    }
}