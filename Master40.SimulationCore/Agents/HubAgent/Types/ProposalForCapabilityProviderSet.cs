using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalForCapabilityProviderSet : List<ProposalForCapabilityProvider>
    {
        public List<ProposalForCapabilityProvider> GetValidProposal()
        {
            return this.Where(x => x.NoPostponed()).ToList();
        }

        public int RequiredProposals => this.Sum(x => x.RequiredProposals);

        public bool AllProposalsReceived => this.TrueForAll(x => x.AllProposalsReceived());

        public bool AnyProposalReady => this.Any(x => x.NoPostponed());

        public long PostponedUntil => this.Min(x => x.PostponedUntil());

        public int ReceivedProposals => this.Sum(x => x.ReceivedProposals);
    }
}
