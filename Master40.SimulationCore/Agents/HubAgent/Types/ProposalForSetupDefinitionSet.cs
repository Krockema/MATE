using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForSetupDefinitionSet : List<ProposalForSetupDefinition>
    {
        public ProposalForSetupDefinition GetValidProposal()
        {
            var allNotPostponed = this.Where(x => x.NoPostponed()).ToList();
            var firstValidProposal = allNotPostponed.OrderBy(x => x.EarliestStart()).FirstOrDefault();
            return firstValidProposal;
        }

        public int RequiredProposals => this.Sum(x => x.RequiredProposals);

        public bool AllProposalsReceived => this.TrueForAll(x => x.AllProposalsReceived());

        public bool AnyProposalReady => this.Any(x => x.NoPostponed());

        public long PostponedUntil => this.Min(x => x.PostponedUntil());

        public int ReceivedProposals => this.Sum(x => x.ReceivedProposals);
    }
}
