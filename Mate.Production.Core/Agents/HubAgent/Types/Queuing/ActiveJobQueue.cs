using Mate.DataCore.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static IJobs;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{
    public class ActiveJobQueue
    {
        public Guid QueueId { get; }
        public M_ResourceCapabilityProvider ResourceCapabilityProvider { get; }
        public JobQueue JobQueue { get; private set; }
        public IJob CurrentJob { get; private set; }
        public List<M_Resource> GetAllResources => this.ResourceCapabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical).Select(x => x.Resource).ToList();

        public List<M_Resource> GetOnlyMainResources => this.ResourceCapabilityProvider.ResourceSetups
            .Where(x => x.UsedInSetup && x.UsedInProcess && x.Resource.IsPhysical).Select(x => x.Resource).ToList();

        public List<M_Resource> GetSetupResources => this.ResourceCapabilityProvider.ResourceSetups
            .Where(x => x.UsedInSetup && x.Resource.IsPhysical).Select(x => x.Resource).ToList();

        public List<M_Resource> GetOnlySetupResources => this.ResourceCapabilityProvider.ResourceSetups
            .Where(x => x.UsedInSetup && !x.UsedInProcess && x.Resource.IsPhysical).Select(x => x.Resource).ToList();

        public List<M_Resource> GetProcessingResources => this.ResourceCapabilityProvider.ResourceSetups
            .Where(x => x.UsedInProcess && x.Resource.IsPhysical).Select(x => x.Resource).ToList();
        public List<M_Resource> GetOnlyProcessingResources => this.ResourceCapabilityProvider.ResourceSetups
            .Where(x => x.UsedInProcess && !x.UsedInSetup && x.Resource.IsPhysical).Select(x => x.Resource).ToList();
        public Dictionary<int, bool> _setupDictionary { get; } = new Dictionary<int, bool>();

        public bool IsSetupFinish => _setupDictionary.All(x => x.Value);

        public Dictionary<int, bool> _processingDictionary { get; } = new Dictionary<int, bool>();
        public bool IsProcessingFinish => _processingDictionary.All(x => x.Value.Equals(true));

        public ActiveJobQueue(M_ResourceCapabilityProvider resourceCapability, JobQueue jobQueue)
        {
            QueueId = Guid.NewGuid();
            ResourceCapabilityProvider = resourceCapability;
            JobQueue = jobQueue;
            CurrentJob = null;
        }

        public IJob Peek(long currentTime)
        {
            return JobQueue.PeekNext(currentTime);
        }


        public IJob Dequeue(long currentTime)
        {
            CurrentJob = JobQueue.DequeueNext(currentTime);
            return CurrentJob;
        }

        public long GetSetupTime()
        {
            return ResourceCapabilityProvider.ResourceSetups.Where(x => x.UsedInSetup).Sum(x => x.SetupTime);
        }

        internal void AddSetup(int resourceId)
        {
            _setupDictionary.Add(resourceId, false);
        }

        internal void FinishSetup(int resourceId)
        {
            if (!_setupDictionary.ContainsKey(resourceId))
            {
                throw new Exception("Setup for resource does not exits");
            }
            
            _setupDictionary[resourceId] = true;
        }
        internal void AddProcessing(int resourceId)
        {
            _processingDictionary.Add(resourceId, false);
        }

        internal void FinishProcessing(int resourceId)
        {
            if (!_processingDictionary.ContainsKey(resourceId))
            {
                throw new Exception($"Processing entry for resource {resourceId} does not exits");
            }

            _processingDictionary[resourceId] = true;
        }

        internal void ClearProcessing()
        {
            _processingDictionary.Clear();
        }


    }
}
