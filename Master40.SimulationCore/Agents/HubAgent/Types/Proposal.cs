using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using static FProposals;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinition
    {
        private FSetupDefinition _fSetupDefinition { get; set; } // allways setup that have been Requested. (No parent.)

        public FSetupDefinition GetFSetupDefinition => _fSetupDefinition;

        private List<FProposal> _proposals = new List<FProposal>();
        public long SetupKey => _fSetupDefinition.SetupKey;

        public ProposalForSetupDefinition(FSetupDefinition fSetupDefinition)
        {
            _fSetupDefinition = fSetupDefinition;
        }

        public bool AllProposalsReceived()
        {
            return _fSetupDefinition.RequiredResources.Count == _proposals.Count;
        }

        public bool AnyPostponed()
        {
            return _proposals.Any(x => x.Postponed.IsPostponed);
        }

        public long PostponedUntil()
        {
            return _proposals.Max(x => x.Postponed.Offset);
        }

        public long EarliestStart()
        {
            return _proposals.Max(x => x.PossibleSchedule);
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

        public int RequiredProposals => _fSetupDefinition.RequiredResources.Count();

        public void RemoveAll()
        {
            _proposals.Clear();
        }

    }
}