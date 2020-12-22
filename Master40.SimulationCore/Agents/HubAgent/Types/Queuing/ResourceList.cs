using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Nominal.Model;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Queuing
{
    public class ResourceState
    {
        public int _resourceId { get; }
        public string _resourceName { get; }
        public IActorRef _resourceRef { get; }
        public ResourceType _resourceType { get; }
        public string _resourceParentCapabilityName { get; }
        public int _resourceParentCapability { get; }
        public M_ResourceCapability _currentResourceCapability { get; private set; }
        public IJob _jobInProgress { get; private set; }

        public bool IsWorking => _jobInProgress != null;

        public ResourceState(int resourceId, string resourceName, IActorRef resourceRef, ResourceType resourceType, string resourceParentCapabilityName, int resourceParentCapability)
        {
            _resourceId = resourceId;
            _resourceName = resourceName;
            _resourceRef = resourceRef;
            _resourceType = resourceType;
            _resourceParentCapabilityName = resourceParentCapabilityName;
            _resourceParentCapability = resourceParentCapability;
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
