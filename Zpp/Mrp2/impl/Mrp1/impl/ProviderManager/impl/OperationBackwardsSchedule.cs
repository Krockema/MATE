using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Util;

namespace Zpp.Scheduling.impl
{
    /**
     * This is only for initial scheduling within mrp1
     */
    public class OperationBackwardsSchedule
    {
        private readonly DueTime _timeBetweenOperations;
        private const int TRANSITION_TIME_FACTOR = 3;

        private readonly Duration _duration;
        private readonly DueTime _startBackwards;
        private readonly DueTime _endBackwards;

        private readonly DueTime _startOfOperation;
        private readonly DueTime _endOfOperation;

        private readonly HierarchyNumber _hierarchyNumber;

        public OperationBackwardsSchedule(DueTime dueTime, Duration duration,
            HierarchyNumber hierarchyNumber)
        {
            if (dueTime == null || duration == null || hierarchyNumber == null)
            {
                throw new MrpRunException("Every parameter must NOT be null.");
            }


            _duration = duration;
            _hierarchyNumber = hierarchyNumber;

            // add transition time aka timeBetweenOperations
            _timeBetweenOperations = new DueTime(CalculateTransitionTime(_duration));
            _endBackwards = dueTime;
            _startBackwards = _endBackwards.Minus(new DueTime(duration.GetValue()));

            _startOfOperation = _startBackwards.Minus(_timeBetweenOperations);
            _endOfOperation = _endBackwards;
        }

        public DueTime GetStartBackwards()
        {
            return _startBackwards;
        }

        public DueTime GetEndBackwards()
        {
            return _endBackwards;
        }

        public DueTime GetEndOfOperation()
        {
            return _endOfOperation;
        }

        public DueTime GetStartOfOperation()
        {
            return _startOfOperation;
        }

        public HierarchyNumber GetHierarchyNumber()
        {
            return _hierarchyNumber;
        }

        public static int GetTransitionTimeFactor()
        {
            return TRANSITION_TIME_FACTOR;
        }

        public static int CalculateTransitionTime(Duration duration)
        {
            return TRANSITION_TIME_FACTOR * duration.GetValue();
        }
    }
}