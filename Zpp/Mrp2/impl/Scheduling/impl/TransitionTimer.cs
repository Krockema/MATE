using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.Mrp2.impl.Scheduling.impl
{
    public class TransitionTimer
    {
        private readonly DueTime _timeBetweenOperations;
        private const int TRANSITION_TIME_FACTOR = 3;
        

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