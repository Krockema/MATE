using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Master40.DB.GanttPlanModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ResourceManager
    {
        //Each Resource (workcenter, prt and worker) has their unique Id 
        public List<ResourceState> resourceWorkList { get; } = new List<ResourceState>();

        public ResourceManager()
        {

        }

        public void Add(string resourceName, string id, IActorRef agentRef)
        {
            var resourceState = new ResourceState(resourceName, id, agentRef);
            resourceWorkList.Add(resourceState);
        }

        public bool ResourceIsWorking(string resourceId)
        {
            return resourceWorkList.Single(x => x.Id.Equals(resourceId)).IsWorking;
        }

        public bool StartActivityAtResource(string resourceName,
            GptblProductionorderOperationActivity productionorderOperationActivity)
        {
            var resource = resourceWorkList.Single(x => x.Name.Equals(resourceName));
            if (resource.IsWorking)
            {
                return false;
            }

            resource.StartActivityAtResource(productionorderOperationActivity);

            return resource.IsWorking;
        }

        public void FinishActivityAtResource(string resourceName)
        {
            resourceWorkList.Single(x => x.Name.Equals(resourceName)).FinishActivityAtResource();
        }

    }
}
