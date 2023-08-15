﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{
    public class JobManager
    {
        private List<IJob> _pendingJobList { get; set; } = new List<IJob>();
        private CapabilityJobStorage _capabilityJobStorage { get; }
        private List<ActiveJobQueue> _activeJobList { get; } = new List<ActiveJobQueue>();

        public JobManager()
        {
            _capabilityJobStorage = new CapabilityJobStorage();
        }

        public void SetJob(IJob job, DateTime currentTime)
        {

            _pendingJobList.RemoveAll(x => x.Key.Equals(job.Key));

            if (job.StartCondition.Satisfied)
            {
                _capabilityJobStorage.Add(job, currentTime);
                return;
            }

            _pendingJobList.Add(job);
        }

        public List<JobQueue> GetAllJobQueues(DateTime currentTime, List<int> availableCapabilities)
        {
            return _capabilityJobStorage.GetAllJobQueues(currentTime: currentTime, availableCapabilities);
        }

        public IJob GetJob(Guid key)
        {
            return _pendingJobList.SingleOrDefault(x => x.Key.Equals(key));
        }

        public Guid AddActiveJob(JobQueue jobQueue, M_ResourceCapabilityProvider capabilityProvider)
        {
            var activeJob = new ActiveJobQueue(capabilityProvider,jobQueue);
            _activeJobList.Add(activeJob);
            return activeJob.QueueId;
        }

       internal ActiveJobQueue GetActiveJob(Guid queueKey)
       {
           return _activeJobList.Single(x => x.QueueId.Equals(queueKey));
        }

       internal bool RemoveActiveJob(Guid queueKey)
       {
           var activeJob = _activeJobList.Single(x => x.QueueId.Equals(queueKey));
           return _activeJobList.Remove(activeJob);
       }

        internal void Remove(JobQueue jobQueue)
        {
            _capabilityJobStorage.Remove(jobQueue);
        }
    }
}
