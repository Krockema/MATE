using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    class ProposalManager : IProposalManager
    {
        List<SetupProposals> setupProposals = new List<SetupProposals>();

        ProposalManager()
        {

        }
        public bool AddProposal(FProposals.FProposal proposal)
        {
            throw new NotImplementedException();
        }

        public bool RemoveProposal(FProposals.FProposal proposal)
        {
            throw new NotImplementedException();
        }
    }
}
