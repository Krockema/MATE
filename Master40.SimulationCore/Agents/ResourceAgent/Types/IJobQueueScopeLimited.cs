using System;
using System.Collections.Generic;
using Master40.DB.DataModel;
using static FBuckets;
using static FJobConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public interface IJobQueueScopeLimited
    {
        FJobConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null);
        FJobConfirmation GetFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability);
        void Enqueue(FJobConfirmation jobConfirmation);
        bool HasQueueAbleJobs();
        QueueingPosition GetQueueAbleTime(IJob job, long currentTime, long processingQueueLength, long resourceIsBlockedUntil, long setupDuration = 0);
        HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation);
        bool CapacitiesLeft();
        T GetJobAs<T>(Guid key);
        IEnumerable<T> GetJobsAs<T>();
        int Limit { get; set; }
        int Count { get; }
        FJobConfirmation GetConfirmation(Guid key);
    }
}