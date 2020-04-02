using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinitionSet
    {
        private List<ProposalForSetupDefinition> _listOfProposalForSetupDefinitions { get; set; } = new List<ProposalForSetupDefinition>();

        public SetupDefinition AssignedSetupDefinition { get; private set; }

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

        internal void ResetAssignedSetupDefinition()
        {
            AssignedSetupDefinition = null;
        }

        internal void SetAssignedSetupDefintion(SetupDefinition setupDefinition)
        {
            AssignedSetupDefinition = setupDefinition;
        }
    }
}
