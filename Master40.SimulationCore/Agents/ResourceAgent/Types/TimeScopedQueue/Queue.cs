using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using static FBuckets;
using static FJobConfirmations;
using static IJobs;

namespace Master40.Tools.TimeScopedQueue
{
    public class Queue : LimitedQueue, IJobQueueScopeLimited
    {

        public Queue(int limit) : base(limit)
        {
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

        public FBucket GetBucket(Guid bucketKey)
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

        public bool HasQueueAbleJobs()
        {
            return this.JobConfirmations.Any(x => x.Job.StartConditions.Satisfied);
        }

        public override bool CapacitiesLeft()
        {
            throw new NotImplementedException();
        }
    }
}
