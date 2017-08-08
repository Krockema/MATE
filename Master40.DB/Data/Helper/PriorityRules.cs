namespace Master40.DB.Data.Helper
{
    public class PriorityRules
    {
        public static int ActivitySlack(int currentTime, int processDuration, int processDue)
        {
            return processDue - processDuration - currentTime;
        }
    }
}