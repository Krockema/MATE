using System;
using System.Collections.Generic;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal.Model;
using static IJobs;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{
    public class ResourceState
    {
        public int _resourceId { get; }
        public string _resourceName { get; }
        public IActorRef _resourceRef { get; }
        public ResourceType _resourceType { get; }
        public string _resourceParentCapabilityName { get; }
        public int _resourceParentCapability { get; }
        public List<int> _resourceCapabilities { get; }
        public M_ResourceCapability _currentResourceCapability { get; private set; }
        public IJob _jobInProgress { get; private set; }

        public bool IsWorking => _jobInProgress != null;

        public ResourceState(int resourceId, string resourceName, IActorRef resourceRef, ResourceType resourceType, string resourceParentCapabilityName, int resourceParentCapability, List<int> resourceCapabilities)
        {
            _resourceId = resourceId;
            _resourceName = resourceName;
            _resourceRef = resourceRef;
            _resourceType = resourceType;
            _resourceParentCapabilityName = resourceParentCapabilityName;
            _resourceParentCapability = resourceParentCapability;
            _resourceCapabilities = resourceCapabilities;
            _jobInProgress = null;
            _currentResourceCapability = null;
        }

        public void SetJob(IJob job)
        {
            _jobInProgress = job;
        }

        internal void ResetJob()
        {
            _jobInProgress = null;
        }

        internal void SetupResource(M_ResourceCapability resourceCapability)
        {
            _currentResourceCapability = resourceCapability;
        }

        internal void SetCapability(M_ResourceCapability capability)
        {
            throw new NotImplementedException();
        }
    }
}
