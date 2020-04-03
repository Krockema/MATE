using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static FOperations;
using static FProposals;
using static FSetupDefinitions;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalManager
    {
        private Dictionary<Guid, ProposalForSetupDefinitionSet> _proposalDictionary { get; set; } = new Dictionary<Guid, ProposalForSetupDefinitionSet>();

        public ProposalManager()
        {

        }

        public bool AllProposalForSetupDefinitionReceived(Guid jobKey)
        {
            return _proposalDictionary.TryGetValue(jobKey, out var proposalForSetupDefinitionSet) 
                   && proposalForSetupDefinitionSet.TrueForAll(x => x.AllProposalsReceived());
        }

        public bool AllSetupDefintionsPostponed(Guid jobKey)
        {
            return _proposalDictionary.TryGetValue(jobKey, out var proposalForSetupDefinitionSet)
                   && proposalForSetupDefinitionSet.TrueForAll(x => x.AnyPostponed());
        }

        public long PostponedUntil(Guid job)
        {
            _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet);

            return proposalForSetupDefinitionSet.Min(x => x.PostponedUntil());
        }

        public bool Add(Guid jobKey, List<FSetupDefinition> fSetupDefinitions)
        {
            var defs = new ProposalForSetupDefinitionSet();
            fSetupDefinitions.ForEach(x => defs.Add(new ProposalForSetupDefinition(x)));
            return _proposalDictionary.TryAdd(jobKey, defs);
        }

        public bool AddProposal(FProposal fProposal)
        {
            
            if (!_proposalDictionary.TryGetValue(fProposal.JobKey, out var proposalForSetupDefinitionSet))
                return false;

            var proposalForSetupDefinition = proposalForSetupDefinitionSet.Single(x => x.GetFSetupDefinition.SetupKey == fProposal.SetupId);

            proposalForSetupDefinition.Add(fProposal);
                return true;
        }

        public ProposalForSetupDefinitionSet GetProposalForSetupDefinitionSet(Guid jobKey)
        {
            return _proposalDictionary.SingleOrDefault(x => x.Key == jobKey).Value;
        }

        internal bool RemoveAllProposalsFor(Guid job)
        {
            if (!_proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet))
                return false;

            proposalForSetupDefinitionSet.ForEach(x => x.RemoveAll());
            return true;
        }

        public bool Remove(Guid jobKey)
        {
            return _proposalDictionary.Remove(jobKey);
        }

        internal ProposalForSetupDefinition GetValidProposalForSetupDefinitionFor(Guid jobKey)
        {
            _proposalDictionary.TryGetValue(jobKey, out var proposalForSetupDefinitionSet);
            return proposalForSetupDefinitionSet?.GetValidProposal();
        }
    }
}
