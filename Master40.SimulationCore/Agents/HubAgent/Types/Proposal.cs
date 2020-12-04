using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static FProposals;
using static FQueueingScopes;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForCapabilityProvider
    {
        private M_ResourceCapabilityProvider _capabilityProvider { get; set; } // allways setup that have been Requested. (No parent.)

        public M_ResourceCapabilityProvider GetCapabilityProvider => _capabilityProvider;

        private List<FProposal> _proposals = new List<FProposal>();
        public int ProviderId => _capabilityProvider.Id;
        public int RequiredProposals => _capabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical).Count();
        public int ReceivedProposals => _proposals.Count();
        public ProposalForCapabilityProvider(M_ResourceCapabilityProvider capabilityProvider)
        {
            _capabilityProvider = capabilityProvider;
        }

        public void ClearAllNotQueueAbleProposals()
        {
            foreach (var proposal in _proposals)
            {
                ((List<FQueueingScope>) proposal.PossibleSchedule).RemoveAll(x => !x.IsQueueAble);
            }
        }

        public List<FProposal> GetProposalsFor(List<IActorRef> actorRefs)
        {
            var proposals = new List<FProposal>();
            ClearAllNotQueueAbleProposals();
            actorRefs.ForEach(x => proposals.AddRange(_proposals.Where(y => y.ResourceAgent.Equals(x)
                                                                           && !y.Postponed.IsPostponed)));
                                                                            
            return proposals;
        }

        public List<IActorRef> GetResources(bool usedInSetup, bool usedInProcess)
        {
            return _capabilityProvider.ResourceSetups.Where(
                            x => x.UsedInSetup == usedInSetup
                            && x.UsedInProcess  == usedInProcess
                            && x.Resource.IResourceRef != null)
                .Select(x => x.Resource.IResourceRef).Cast<IActorRef>().ToList();
        }

        public List<IActorRef> GetAllResources()
        {
            return _capabilityProvider.ResourceSetups.Where(x => x.Resource.IResourceRef != null)
                .Select(x => x.Resource.IResourceRef).Cast<IActorRef>().ToList();
        }

        public bool AllProposalsReceived()
        {
            return _capabilityProvider.ResourceSetups.Where(x => x.Resource.IsPhysical).Count() == _proposals.Count;
        }

        public bool NoPostponed()
        {
            return _proposals.TrueForAll(x => !x.Postponed.IsPostponed);
        }

        public long PostponedFor()
        {
            return _proposals.Max(x => x.Postponed.Offset);
        }

        public long EarliestStart()
        {
            // 1.  


            return _proposals.Max(x => (int)x.PossibleSchedule);
        }

        public void Add(FProposal proposal)
        {
            if (_proposals.Any(x => x.ResourceAgent == proposal.ResourceAgent))
            {
                throw new Exception("proposal for resourceAgent already exits");
            }
            // check if proposal for same resource already exits --> override ??
            _proposals.Add(proposal);
        }
        public void RemoveAll()
        {
            _proposals.Clear();
        }

    }
}