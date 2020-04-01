using System.Collections.Generic;
using Master40.DB.DataModel;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class Proposal
    {
        private List<FProposal> _proposals = new List<FProposal>();
        public long _setupId { get; private set;} // allways setup that have been Requested. (No parent.)

        public Proposal(long setupId)
        {
            _setupId = setupId;
        }

        public void Add(FProposal proposal)
        {
            _proposals.Add(proposal);
        }

    }
}