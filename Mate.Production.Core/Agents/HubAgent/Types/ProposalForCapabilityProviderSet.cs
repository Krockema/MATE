using System.Collections.Generic;
using System.Linq;

namespace Mate.Production.Core.Agents.HubAgent.Types
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

        public long PostponedFor => this.Min(x => x.PostponedFor());

        public int ReceivedProposals => this.Sum(x => x.ReceivedProposals);
    }
}
