using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using Akka.Actor;
using static FJobConfirmations;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;
using static IConfirmations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public interface IJobQueue
    {
        IConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null);
        IConfirmation GetFirstIfSatisfied(long currentTime, M_ResourceCapability resourceCapability = null);
        IConfirmation GetFirstIfSatisfiedAndSetReadyAtIsSmallerOrEqualThen(long currentTime, M_ResourceCapability resourceCapability = null); 
        void Enqueue(IConfirmation jobConfirmation);
        bool HasQueueAbleJobs();
        bool FirstJobIsQueueAble();
        List<FQueueingScope> GetQueueAbleTime(FRequestProposalForCapability jobProposal
                                                , long currentTime
                                                , CapabilityProviderManager cpm
                                                , long resourceBlockedUntil
                                                , IActorRef resourceRef);
        HashSet<IConfirmation> GetTail(long currentTime, IConfirmation jobConfirmation);
        bool CapacitiesLeft();
        T GetJobAs<T>(Guid key);
        IEnumerable<T> GetJobsAs<T>();
        int Limit { get; set; }
        int Count { get; }
        IConfirmation GetConfirmation(Guid key);
        IConfirmation FirstOrNull();
        bool RemoveJob(IConfirmation job);
        long Workload { get; }
        bool CheckScope(IConfirmation fJobConfirmation, long time, long resourceIsBusyUntil, int equippedCapability, bool usedInSetup);
        HashSet<IConfirmation> GetAllSubsequentJobs(long getScopeStart);
        HashSet<IConfirmation> GetAllJobs();
        bool UpdateBucket(IJob job);
        bool QueueHealthCheck(long currentTime);
    }
}