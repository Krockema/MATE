using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal.Model;
using static IJobs;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{
    public class ResourceManager
    {
        public List<ResourceState> _resourceList { get; }

        public ResourceManager()
        {
            _resourceList = new List<ResourceState>();
        }

        public void Add(int resourceId, string resourceName, IActorRef resourceRef, ResourceType resourceType, string resourceCapabilityName, int resourceCapability, List<int> resourceCapabilities)
        {
            if (_resourceList.Exists(x => x._resourceId.Equals(resourceId)))
            {
                //Resource does already exits, create only capabilities
                return;
            }
            
            _resourceList.Add(new ResourceState(resourceId, resourceName, resourceRef, resourceType, resourceCapabilityName, resourceCapability, resourceCapabilities));
        }

        internal List<int> GetAvailableCapabilities()
        {
            List<int> availableCapabilities = new List<int>();
            _resourceList.Where(x => x._resourceType.Equals(ResourceType.Workcenter) && x.IsWorking == false).ForEach(x => availableCapabilities.AddRange(x._resourceCapabilities));
            return availableCapabilities; 
        }

        internal bool ResouresAreWorking(List<M_Resource> requiredResources)
        {
            var resourceStates = new List<ResourceState>();
            foreach (var resource in requiredResources)
            {
                var state =_resourceList.Single(x => x._resourceId.Equals(resource.Id));
                resourceStates.Add(state);
            }

            return resourceStates.Any(x => x.IsWorking);
        }

        internal void SetSetup(int resourceId, M_ResourceCapability resourceCapability)
        {
            _resourceList.Single(x => x._resourceId.Equals(resourceId)).SetupResource(resourceCapability);
        }

        internal void SetJob(int resourceId, IJob job)
        {
            _resourceList.Single(x => x._resourceId.Equals(resourceId)).SetJob(job);
        }

        internal void ResetJob(int resourceId)
        {
            _resourceList.Single(x => x._resourceId.Equals(resourceId)).ResetJob();
        }

        internal List<ResourceState> GetResourceStates(List<int> resourceIds)
        {
            var resourceStateList = new List<ResourceState>();

            foreach (var resourceId in resourceIds)
            {
                resourceStateList.Add(_resourceList.Single(x => x._resourceId.Equals(resourceId)));
            }

            return resourceStateList;

        }

        internal bool RequireSetup(List<int> mainResources)
        {
            throw new NotImplementedException();
        }

        internal void SetJobQueue(IJob job, List<M_Resource> requiredResources)
        {
            var resourceStates = GetResourceStates(requiredResources.Select(x=> x.Id).ToList());

            resourceStates.ForEach(x => x.SetJob(job));
        }
    }
}
