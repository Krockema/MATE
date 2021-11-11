using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.GanttPlan.GanttPlanModel;

namespace Mate.Production.Core.Agents.HubAgent.Types.Central
{
    public class ResourceManager
    {
        //Each Resource (workcenter, prt and worker) has their unique Id 
        public List<ResourceState> resourceStateList { get; } = new List<ResourceState>();

        public ResourceManager()
        {

        }

        public void Add(ResourceDefinition resourceDefinition)
        {
            var resourceState = new ResourceState(resourceDefinition);
            resourceStateList.Add(resourceState);
        }
        public bool ResourceReadyToWorkOn(int resourceId, string key)
        {
            var resource = resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId));

            if(resource.IsWorking)
                return false;

            var nextActivity = resource.ActivityQueue.Peek();
            if (nextActivity == null)
            {
                System.Diagnostics.Debug.WriteLine($"No next activity for {resource.ResourceDefinition.Name} in resource activity queue");
                return false;
            }
            var isNextActivityEqual = nextActivity.GetKey == key;
            if (!isNextActivityEqual)
                System.Diagnostics.Debug.WriteLine($"Next activity on {resource.ResourceDefinition.Name} should be {key} but is {nextActivity.GetKey}");

            return isNextActivityEqual;

        }

        public bool ResourceIsWorking(int resourceId)
        {
            return resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId)).IsWorking;
        }

        public bool StartActivityAtResource(int resourceId,
            GptblProductionorderOperationActivity productionorderOperationActivity)
        {
            var resource = resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId));
            if (resource.IsWorking)
            {
                return false;
            }

            resource.StartActivityAtResource(productionorderOperationActivity);

            return resource.IsWorking;
        }

        public void FinishActivityAtResource(int resourceId)
        {
            resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId)).FinishActivityAtResource();
        }

        public GptblProductionorderOperationActivity GetCurrentActivity(int resourceId)
        {
            return resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId))
                .CurrentProductionOrderActivity;
        }

        public static void ShowCurrentWork(List<ResourceState> temresourceStateList)
        {
            var t = temresourceStateList.Where(x => x.GetCurrentProductionOperationActivity != null).ToList();
            foreach (var x in t)
            {
                System.Diagnostics.Debug.WriteLine("" + x.ResourceDefinition.Name +
                                                   x.GetCurrentProductionOperationActivity);
            }
        }

        public static void ShowNextWork(List<ResourceState> temresourceStateList)
        {
            foreach (var x in temresourceStateList)
            {
                if (x.GetCurrentProductionOperationActivity != null)
                {

                    System.Diagnostics.Debug.WriteLine("Has activity in currentWork" + x.ResourceDefinition.Name +
                                                       x.GetCurrentProductionOperationActivity);
                }
                else
                {
                    if (x.ActivityQueue.Count < 1)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"NextQueueItem {x.ResourceDefinition.Name} has no activity in queue");

                        continue;
                    }
                    var nextQueueItem = x.ActivityQueue.Peek();
                    
                    System.Diagnostics.Debug.WriteLine(
                        $"NextQueueItem {x.ResourceDefinition.Name}|{nextQueueItem.ProductionorderId}|{nextQueueItem.OperationId}|{nextQueueItem.ActivityId}");
                   
                }
            }
        }

    }
}
