using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class Hub
    {
        public static class Properties
        {
            public const string WORK_ITEM_QUEUE = "WorkItemQueue";
            public const string MACHINE_AGENTS = "MachineAgents";
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
