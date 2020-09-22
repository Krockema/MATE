using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Akka.Actor;
using Master40.DB.GanttPlanModel;
using Master40.DB.Nominal.Model;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
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

        public bool ResourceIsWorking(string resourceId)
        {
            return resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId)).IsWorking;
        }

        public bool StartActivityAtResource(string resourceId,
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

        public void FinishActivityAtResource(string resourceId)
        {
            resourceStateList.Single(x => x.ResourceDefinition.Id.Equals(resourceId)).FinishActivityAtResource();
        }

        public GptblProductionorderOperationActivity GetCurrentActivity(string resourceId)
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
                    var nextQueueItem = x.ActivityQueue.Peek();
                    if (nextQueueItem == null)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"NextQueueItem {x.ResourceDefinition.Name} has no activity in queue");

                        continue;
                        
                    }
                    System.Diagnostics.Debug.WriteLine(
                        $"NextQueueItem {x.ResourceDefinition.Name}|{nextQueueItem.ProductionorderId}|{nextQueueItem.OperationId}|{nextQueueItem.ActivityId}");
                   
                }
            }
        }

    }
}
