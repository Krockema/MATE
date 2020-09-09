using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.GanttPlanModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ResourceState
    {
        public string Name { get; private set;  }

        public GptblProductionorderOperationActivity CurrentProductionOrderActivity { get; private set; }
        
        public bool IsWorking => CurrentProductionOrderActivity != null;

        public bool FinishedWork { get; private set; }

        public ResourceState(string name)
        {
            Name = name;
            CurrentProductionOrderActivity = null;
        }

        internal void StartActivityAtResource(GptblProductionorderOperationActivity productionorderOperationActivity)
        {
            CurrentProductionOrderActivity = productionorderOperationActivity;
        }

        internal void FinishActivityAtResource()
        {
            FinishedWork = true;
        }

        internal void ResetActivityAtResource()
        {
            CurrentProductionOrderActivity = null;
        }


    }
}
