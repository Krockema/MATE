using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using Akka.Actor;
using static FJobConfirmations;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;
using static IConfirmations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public interface IJobQueue
    {
        IConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null);
        IConfirmation DequeueFirstIfSatisfied(long currentTime, M_ResourceCapability resourceCapability = null);
        void Enqueue(IConfirmation jobConfirmation);
        bool HasQueueAbleJobs();

        List<FQueueingScope> GetQueueAbleTime(FRequestProposalForCapabilityProvider jobProposal
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
        bool CheckScope(IConfirmation fJobConfirmation, long time);
        HashSet<IConfirmation> GetAllSubsequentJobs(long getScopeStart);
        HashSet<IConfirmation> GetAllJobs();

    }
}