namespace Master40.Agents.Agents.Model
{
    public class PriorityRules
    {
        
        public static int HatchingTime(long currentTime, int processDuration, int processDue)
        {
            return processDue - processDuration - (int)currentTime;
        }
    }
}