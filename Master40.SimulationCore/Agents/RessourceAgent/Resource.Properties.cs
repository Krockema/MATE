namespace Master40.SimulationCore.Agents.Ressource
{
    public partial class Resource
    {
        public static class Properties
        {
            public const string RESOURCE = "Resource";
            public const string PROGRESS_QUEUE_SIZE = "ProgressQueueSize"; // int
            public const string QUEUE = "Queue"; // List<FBucket> or LimitedQueue<WorkItem>
            public const string PROCESSING_QUEUE = "ProcessingQueue"; // LimitedQueue<FBucket> or LimitedQueue<WorkItem>
            public const string QUEUE_LENGTH = "QueueLength"; // int
            public const string ITEMS_IN_PROGRESS = "ItemInProgress"; // bool
            public const string WORK_TIME_GENERATOR = "WorkTimeGenerator"; // WorkTimeGenerator
            public const string HUB_AGENT_REF = "HubAgentRef"; // IActorRef
            public const string EQUP_RESOURCETOOL = "EquippdedResourcetool"; // M_ResourceTool
            public const string BROKEN = "BREAKDOWN";
        }
    }
}
