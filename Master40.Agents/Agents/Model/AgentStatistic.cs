using System.Collections.Generic;

namespace Master40.Agents.Agents.Model
{
    public class AgentStatistic
    {

        public string AgentId { get; set; }
        public string AgentType { get; set; }
        public string AgentName { get; set; }
        public long Time { get; set; }
        public long ProcessingTime { get; set; }
        public string CalledMethod { get; set; }
        public static List<string> Log { get; set; }
    }
}