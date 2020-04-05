using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinitionSet : List<ProposalForSetupDefinition>
    {
        public ProposalForSetupDefinition GetValidProposal()
        {
            return this.FirstOrDefault(x => this.Min(y => y.EarliestStart()) == x.EarliestStart()
                             && x.NoPostponed());
        }

        public int RequiredProposals => this.Sum(x => x.RequiredProposals);

        public bool AllProposalsReceived => this.TrueForAll(x => x.AllProposalsReceived());

        public bool AnyProposalReady => this.Any(x => x.NoPostponed());

        public long PostponedUntil => this.Min(x => x.PostponedUntil());

        public int ReceivedProposals => this.Sum(x => x.ReceivedProposals);
    }
}
