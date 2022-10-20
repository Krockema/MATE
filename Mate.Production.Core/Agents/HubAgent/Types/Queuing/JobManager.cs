using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.DataModel;
using static IJobs;

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

        public void SetJob(IJob job, long currentTime)
        {

            _pendingJobList.RemoveAll(x => x.Key.Equals(job.Key));

            if (job.StartConditions.Satisfied)
            {
                _capabilityJobStorage.Add(job, currentTime);
                return;
            }

            _pendingJobList.Add(job);

        }

        public void Add(IJob job)
        {
            _pendingJobList.Add(job);
        }

        public List<JobQueue> GetNextJobQueues(long currentTime)
        {
            return _capabilityJobStorage.GetJobQueues(currentTime);
        }

        public List<JobQueue> GetAllJobQueues(long currentTime, List<int> availableCapabilities)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="currentTime"></param>
        /// <returns>
        /// List<Jobs> followingJobs</Jobs>
        /// int counterPreviousJobs
        /// long durationPreviousJobs
        /// </returns>
        public (List<IJob>, int, long) GetPostionOfJob(IJob job, long currentTime)
        {
            //int counterPreviousJobs = _activeJobList.Sum(x => x.JobQueue.Count());
            //long durationPreviousJobs = _activeJobList.Sum(x => x.JobQueue.Sum(y => y.Duration));
            var counterPreviousJobs = 0;
            var durationPreviousJobs = 0L;
            var followingJobs = new List<IJob>();

            var capabilityQueue = _capabilityJobStorage.GetJobQueue(job.RequiredCapability.Id);

            if(capabilityQueue != null)
            { 
                var previousJobs = capabilityQueue.Where(x => x.Priority(currentTime) <= job.Priority(currentTime));

                counterPreviousJobs = previousJobs.Count();
                durationPreviousJobs = previousJobs.Sum(x => x.Duration);

                followingJobs = capabilityQueue.Where(x => x.Priority(currentTime) > job.Priority(currentTime)).ToList();
            }
            return (followingJobs, counterPreviousJobs, durationPreviousJobs);


        }

    }
}
