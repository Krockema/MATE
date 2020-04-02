using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinitionSet
    {
        public List<ProposalForSetupDefinition> _listOfProposalForSetupDefinitions { get; private set; } = new List<ProposalForSetupDefinition>();

        public FSetupDefinition AssignedSetupDefinition { get; private set; }

        public ProposalForSetupDefinitionSet()
        {
            AssignedSetupDefinition = null;
        }

        internal void Add(ProposalForSetupDefinition proposalForSetupDefinition)
        {
            _listOfProposalForSetupDefinitions.Add(proposalForSetupDefinition);
        }

        internal List<ProposalForSetupDefinition> getAllProposalForSetupDefinitions()
        {
            return _listOfProposalForSetupDefinitions;
        }

        public ProposalForSetupDefinition GetValidProposal()
        {
            return _listOfProposalForSetupDefinitions.First(x => _listOfProposalForSetupDefinitions.Min(y => y.EarliestStart()) == x.EarliestStart());
        }


        internal void ResetAssignedSetupDefinition()
        {
            AssignedSetupDefinition = null;
        }

        internal void SetAssignedSetupDefintion(FSetupDefinition setupDefinition)
        {
            AssignedSetupDefinition = setupDefinition;
        }
    }
}
