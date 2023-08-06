using System;
using System.Collections.Generic;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Scopes;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;

namespace Mate.Production.Core.Agents.ResourceAgent.Types
{
    public interface IJobQueue
    {
        IConfirmation DequeueFirstSatisfied(DateTime currentTime, M_ResourceCapability resourceCapability = null);
        IConfirmation GetFirstIfSatisfied(DateTime currentTime, M_ResourceCapability resourceCapability = null);
        IConfirmation GetFirstIfSatisfiedAndSetReadyAtIsSmallerOrEqualThan(DateTime currentTime, M_ResourceCapability resourceCapability = null); 
        void Enqueue(IConfirmation jobConfirmation);
        bool HasQueueAbleJobs();
        bool FirstJobIsQueueAble();
        List<QueueingScopeRecord> GetQueueAbleTime(RequestProposalForCapabilityRecord jobProposal
                                                , DateTime currentTime
                                                , CapabilityProviderManager cpm
                                                , DateTime resourceBlockedUntil
                                                , int resourceId);
        HashSet<IConfirmation> GetTail(DateTime currentTime, IConfirmation jobConfirmation);
        bool CapacitiesLeft();
        T GetJobAs<T>(Guid key);
        IEnumerable<T> GetJobsAs<T>();
        TimeSpan Limit { get; set; }
        int Count { get; }
        IConfirmation GetConfirmation(Guid key);
        IConfirmation FirstOrNull();
        bool RemoveJob(IConfirmation job);
        TimeSpan Workload { get; }
        bool CheckScope(IConfirmation JobConfirmation, DateTime time, DateTime resourceIsBusyUntil, int equippedCapability, bool usedInSetup);
        HashSet<IConfirmation> GetAllSubsequentJobs(DateTime getScopeStart);
        HashSet<IConfirmation> GetAllJobs();
        bool UpdateBucket(IJob job);
        bool QueueHealthCheck(DateTime currentTime);
    }
}