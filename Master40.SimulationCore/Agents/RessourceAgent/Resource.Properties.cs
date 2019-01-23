using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class Resource
    {
        public static class Properties
        {
            public const string MACHINE = "Machine";
            public const string PROGRESS_QUEUE_SIZE = "ProgressQueueSize"; // int
            public const string QUEUE = "Queue"; // List<WorkItem>
            public const string PROCESSING_QUEUE = "ProcessingQueue"; // LimitedQueue<WorkItem>
            public const string QUEUE_LENGTH = "QueueLength"; // int
            public const string ITEMS_IN_PROGRESS = "ItemInProgress"; // bool
            public const string WORK_TIME_GENERATOR = "WorkTimeGenerator"; // WorkTimeGenerator
            public const string HUB_AGENT_REF = "HubAgentRef"; // IActorRef
        }
    }
}
