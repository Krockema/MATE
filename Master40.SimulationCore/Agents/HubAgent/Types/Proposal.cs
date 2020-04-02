using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinition
    {
        public SetupDefinition _setupDefinition { get; private set; } // allways setup that have been Requested. (No parent.)

        private List<FProposal> _proposals = new List<FProposal>();
        public int SetupKey => _setupDefinition.ResourceSetup.Id;

        public ProposalForSetupDefinition(SetupDefinition setupDefinition)
        {
            _setupDefinition = setupDefinition;
        }

        public bool AllProposalsReceived()
        {
            return _setupDefinition.RequiredResources.Count == _proposals.Count;
        }

        public bool AnyPostponed()
        {
            return _proposals.Any(x => x.Postponed.IsPostponed);
        }

        public long PostponedUntil()
        {
            return _proposals.Max(x => x.Postponed.Offset);
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