using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using static FBuckets;
using static FJobConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue { 
    public class TimeConstraintQueue : SortedList<long, FJobConfirmation>, IJobQueue
    {
        public int Limit { get; set; }
        public FJobConfirmation GetConfirmation(Guid key)
        {
            throw new NotImplementedException();
        }

        public TimeConstraintQueue(int limit)
        {
            Limit = limit;
        }

        public HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation)
        {
            throw new NotImplementedException();
        }

        public FJobConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null)
        {
            throw new NotImplementedException();
        }

        public void Enqueue(FJobConfirmation jobConfirmation)
        {
            throw new NotImplementedException();
        }

        public FJobConfirmation GetFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability)
        {
            throw new NotImplementedException();
        }

        public QueueingPosition GetQueueAbleTime(IJob job, long currentTime, long processingQueueLength, long resourceIsBlockedUntil, long setupDuration = 0)
        {
            throw new NotImplementedException();
        }

        public bool CapacitiesLeft()
        {
            throw new NotImplementedException();
        }

        public bool HasQueueAbleJobs()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetJobsAs<T>()
        {
            return this.Values.Cast<T>();
        }

        public T GetJobAs<T>(Guid key)
        {
            throw new NotImplementedException();
        }
    }
}
