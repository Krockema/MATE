using Master40.SimulationImmutables;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.ProductionAgent
{
    public partial class Production
    {
        public static class Properties
        {
            public const string REQUEST_ITEM = "RequestItem";
            public const string WORK_ITEMS = "WorkItems";
            public const string REQUESTED_ITEMS = "RequestedItems";
            public const string HUB_AGENTS = "HubAgents";
            public const string ELEMENT_STATUS = "ElementStatus";
            public const string NEXT_WORK_ITEM = "NextWorkItem";
            public const string CHILD_WORKITEMS = "ChildWorkItems";
        }

        public class WorkItems : List<IKey>
        {

        }
    }
}
