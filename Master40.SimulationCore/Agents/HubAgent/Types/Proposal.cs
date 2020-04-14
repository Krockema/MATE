using Akka.Actor;
using Master40.DB.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForCapabilityProvider
    {
        private M_ResourceCapabilityProvider _capabilityProvider { get; set; } // allways setup that have been Requested. (No parent.)

        public M_ResourceCapabilityProvider GetCapabilityProvider => _capabilityProvider;

        private List<FProposal> _proposals = new List<FProposal>();
        public int ProviderId => _capabilityProvider.Id;
        public int RequiredProposals => _capabilityProvider.ResourceSetups.Count();
        public int ReceivedProposals => _proposals.Count();
        public ProposalForCapabilityProvider(M_ResourceCapabilityProvider capabilityProvider)
        {
            _capabilityProvider = capabilityProvider;
        }

        public List<FProposal> GetProposalsFor(List<IActorRef> actorRefs)
        {
            var proposals = new List<FProposal>();
            actorRefs.ForEach(x => proposals.AddRange(_proposals.Where(y => y.ResourceAgent.Equals(x))));
            return proposals;
        }

        public bool AllProposalsReceived()
        {
            return _capabilityProvider.ResourceSetups.Sum(x => x.Resource.Count) == _proposals.Count;
        }

        public bool NoPostponed()
        {
            return _proposals.TrueForAll(x => !x.Postponed.IsPostponed);
        }

        public long PostponedUntil()
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