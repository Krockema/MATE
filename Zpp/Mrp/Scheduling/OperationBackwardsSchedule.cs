using Zpp.WrappersForPrimitives;

namespace Zpp.Mrp.Scheduling
{
    public class OperationBackwardsSchedule
    {
        private readonly DueTime _endBackwards;
        private readonly DueTime _startBackwards;
        private readonly HierarchyNumber _hierarchyNumber;

        public OperationBackwardsSchedule()
        {
        }

        public OperationBackwardsSchedule(DueTime endBackwards, DueTime startBackwards, HierarchyNumber hierarchyNumber)
        {
            _endBackwards = endBackwards;
            _startBackwards = startBackwards;
            _hierarchyNumber = hierarchyNumber;
        }

        public DueTime GetStartBackwards()
        {
            return _startBackwards;
        }

        public DueTime GetEndBackwards()
        {
            return _endBackwards;
        }

        public HierarchyNumber GetHierarchyNumber()
        {
            return _hierarchyNumber;
        }
    }
}