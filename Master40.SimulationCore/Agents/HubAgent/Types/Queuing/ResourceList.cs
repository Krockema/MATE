using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Master40.DB.DataModel;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Queuing
{
    public class ResourceState
    {
        public int _resourceId { get; }
        public string _resourceName { get; }
        public IActorRef _resourceRef { get; }
        public string _resourceType { get; }
        public M_ResourceCapability _currentResourceCapability { get; private set; }
        public IJob _jobInProgress { get; private set; }

        public bool IsWorking => _jobInProgress != null;

        public ResourceState(int resourceId, string resourceName, IActorRef resourceRef, string resourceType)
        {
            _resourceId = resourceId;
            _resourceName = resourceName;
            _resourceRef = resourceRef;
            _resourceType = resourceType;
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
