using System;

namespace Mate.DataCore.Data.Helper
{
    public class PriorityRules
    {

        public PriorityRules()
        {
            //_rule = Activity();
        }

        private static Func<object, int> _rule;
        public static int ActivitySlack(int currentTime, int processDuration, int processDue)
        {

            return processDue - processDuration - currentTime;
        }

        public static int Activity(int currentTime, int processDuration, int processDue)
        {
            return processDue - processDuration - currentTime;
        }

        public static int ASTest(int currentTime, int processDuration, int processDue)
        {
            return _rule(new { currentTime, processDuration, processDue });
        }
        public static void SetPriorityRule(Func<object, int> rule)
        {
            _rule = rule;
        }

    }

}