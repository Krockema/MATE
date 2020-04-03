using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinitionSet : List<ProposalForSetupDefinition>
    {
        public ProposalForSetupDefinition GetValidProposal()
        {
            return this.First(x =>
                this.Min(y => y.EarliestStart()) == x.EarliestStart());
        }
    }
}
