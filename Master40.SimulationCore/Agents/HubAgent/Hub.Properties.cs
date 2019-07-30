using Akka.Actor;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public partial class Hub
    {
        public static class Properties
        {
            public const string WORK_ITEM_QUEUE = "WorkItemQueue";
            public const string BUCKETS = "Buckets";
            public const string RESOURCE_AGENTS = "ResourceAgents";
            public const string SKILL_GROUP = "SkillGroup";
        }

        /// <summary>
        /// To track Machine State ( Later use ) 
        /// --> Replacement for new Dictionary<IActorRef, string>()
        /// </summary>
        private class ResourceState
        {
            public bool Online { get; set; }
            public IActorRef actorRef { get; set; }
            public string Name { get; set; }
        }
    }
}
