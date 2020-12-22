using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using static FOperations;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Queuing
{
    // For Ready Elemnts
    public class CapabilityJobStorage
    {
        // Dict of String , Capability 
        private Dictionary<int, JobQueue> _jobStorage;

        public CapabilityJobStorage()
        {
            _jobStorage = new Dictionary<int, JobQueue>();

        }

        public void Add(IJob job, long currentTime)
        {
            if (_jobStorage.TryGetValue(job.RequiredCapability.Id, out var jobQueue))
            {
                jobQueue.Enqueue(job, currentTime);
                return;
            }
            
            jobQueue = new JobQueue();
            jobQueue.Enqueue(job, currentTime);
            _jobStorage.Add(job.RequiredCapability.Id, jobQueue);
            
        }

        public void Remove(JobQueue jobQueue)
        {
            var capabilityId = jobQueue.data.First().RequiredCapability.Id;

            if (!_jobStorage.ContainsKey(capabilityId))
            {
                throw new Exception("JobQueue not in Storage anymore!");
            }
            
            _jobStorage.Remove(capabilityId);

        }

        internal List<JobQueue> GetAllJobQueues(long currentTime, List<int> availableCapabilities)
        {
            return _jobStorage.Values.OrderBy(x => x.Priority(currentTime: currentTime)).ToList();
            //return _jobStorage.Where(x => availableCapabilities.Contains(x.Key)).Select(x => x.Value).OrderBy(x => x.Priority(currentTime: currentTime)).ToList();
        }

        public List<JobQueue> GetJobQueues(long currentTime)
        {
            var jobQueues = new List<JobQueue>();
            var minPrio = double.MaxValue;

            foreach (var item in _jobStorage)
            {
                var job = item.Value;
                var itemPriority = job.Priority(currentTime: currentTime);

                if(itemPriority > minPrio)
                    continue;

                if (itemPriority < minPrio)
                {
                    jobQueues.Clear();
                }

                jobQueues.Add(job);
                minPrio = itemPriority;
            }

            return jobQueues;

        }
        
    }
}
