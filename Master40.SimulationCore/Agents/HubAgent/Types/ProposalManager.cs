using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalManager : IProposalManager
    {
        private List<Proposal> _proposals { get; set; } = new List<Proposal>();

        public ProposalManager()
        {

        }
        public void Add(FProposal proposal)
        {
            var proposalForSetup = _proposals.SingleOrDefault(x => x._setupId == proposal.SetupId);

            if (proposalForSetup == null)
            {
                proposalForSetup = new Proposal(proposal.SetupId);
                _proposals.Add(proposalForSetup);
            }

            proposalForSetup.Add(proposal);

        }

        public void Remove(long setupId)
        {
            var proposalForSetup = _proposals.SingleOrDefault(x => x._setupId == setupId);

            if (proposalForSetup == null)
                return;

            _proposals.Remove(proposalForSetup);
        }
    }
}
