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
        private List<ResourceState> resourceWorkList { get; } = new List<ResourceState>();

        public ResourceManager()
        {

        }

        public void Add(string resourceName)
        {
            var resourceState = new ResourceState(resourceName);
            resourceWorkList.Add(resourceState);
        }

        public bool ResourceIsWorking(string resourceName)
        {
            return resourceWorkList.Single(x => x.Name.Equals(resourceName)).IsWorking;
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
