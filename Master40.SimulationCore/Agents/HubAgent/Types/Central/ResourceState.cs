using Akka.Actor;
using Master40.DB.GanttPlanModel;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ResourceState
    {
        public ResourceDefinition ResourceDefinition { get; private set; }
        public GptblProductionorderOperationActivity CurrentProductionOrderActivity { get; private set; }
        
        public bool IsWorking => CurrentProductionOrderActivity != null;

        public bool FinishedWork { get; private set; }

        public Queue<GptblProductionorderOperationActivityResourceInterval> ActivityQueue { get; set; }

        public string GetCurrentProductionOperationActivity => CurrentProductionOrderActivity != null ? $"ProductionOrderId: {CurrentProductionOrderActivity.ProductionorderId} " +
                                                                                                        $"| Operation: {CurrentProductionOrderActivity.OperationId} " +
                                                                                                        $"| Activity {CurrentProductionOrderActivity.ActivityId}"  
                                                                                                        : null;

        public ResourceState(ResourceDefinition resourceDefinition)
        {
            ResourceDefinition = resourceDefinition;
            CurrentProductionOrderActivity = null;
        }

        internal void StartActivityAtResource(GptblProductionorderOperationActivity productionorderOperationActivity)
        { 
            CurrentProductionOrderActivity = productionorderOperationActivity;
        }

        internal void FinishActivityAtResource()
        {
            ResetActivityAtResource();
            FinishedWork = true;
        }

        internal void ResetActivityAtResource()
        {
            CurrentProductionOrderActivity = null;
        }


    }
}
