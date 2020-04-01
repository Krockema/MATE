using System.Collections.Generic;
using Master40.DB.DataModel;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinition
    {
        private SetupDefinition _setupDefinition { get; set; } // allways setup that have been Requested. (No parent.)

        private List<FProposal> _proposals = new List<FProposal>();


        public ProposalForSetupDefinition(SetupDefinition setupDefinition)
        {
            _setupDefinition = setupDefinition;
        }


        public void Add(FProposal proposal)
        {
            _proposals.Add(proposal);
        }

    }
}