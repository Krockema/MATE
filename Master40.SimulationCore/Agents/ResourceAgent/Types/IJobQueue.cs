using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using static FJobConfirmations;
using static FQueueingScopes;
using static FRequestProposalForCapabilityProviders;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public interface IJobQueue
    {
        FJobConfirmation DequeueFirstSatisfied(long currentTime, M_ResourceCapability resourceCapability = null);
        void Enqueue(FJobConfirmation jobConfirmation);
        bool HasQueueAbleJobs();
        List<FQueueingScope> GetQueueAbleTime(FRequestProposalForCapabilityProvider jobProposal, long currentTime, CapabilityProviderManager cpm);
        HashSet<FJobConfirmation> CutTail(long currentTime, FJobConfirmation jobConfirmation);
        bool CapacitiesLeft();
        T GetJobAs<T>(Guid key);
        IEnumerable<T> GetJobsAs<T>();
        int Limit { get; set; }
        int Count { get; }
        FJobConfirmation GetConfirmation(Guid key);
        FJobConfirmation FirstOrNull();
        bool RemoveJob(FJobConfirmation job);
        long Workload { get; }

        bool CheckScope(FJobConfirmation fJobConfirmation, long time);
    }
}