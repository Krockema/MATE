using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static FOperations;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ProposalManager
    {
        private Dictionary<FOperation, List<ProposalForSetupDefinition>> _proposalDictionary { get; set; } = new Dictionary<FOperation, List<ProposalForSetupDefinition>>();
 
        public ProposalManager()
        {

        }
        
        public bool AllProposalForSetupDefinitionReceived(FOperation fOperation)
        {
            return _proposalDictionary.TryGetValue(fOperation, out var proposalForSetupDefinitions) 
                   && proposalForSetupDefinitions.TrueForAll(x => x.AllProposalsReceived());
        }

        public bool AllSetupDefintionsPostponed(FOperation fOperation)
        {
            return _proposalDictionary.TryGetValue(fOperation, out var proposalForSetupDefinitions)
                   && proposalForSetupDefinitions.TrueForAll(x => x.AllPostponed());
        }

        public long PostponedUntil(FOperation fOperation)
        {
            _proposalDictionary.TryGetValue(fOperation, out var proposalForSetupDefinitions);

            return proposalForSetupDefinitions.Min(x => x.PostponedUntil());
        }

        public bool Add(FOperation fOperation, List<SetupDefinition> setupDefinitions)
        {
            var defs = new List<ProposalForSetupDefinition>();
            setupDefinitions.ForEach(x => defs.Add(new ProposalForSetupDefinition(x)));
            return _proposalDictionary.TryAdd(fOperation, defs);
        }

        public bool AddProposal(FProposal fProposal)
        {
            var fOperation = GetOperationBy(fProposal.JobKey);

            if (!_proposalDictionary.TryGetValue(fOperation, out var proposalForSetupDefinitions))
                return false;

            var proposalForSetupDefinition = proposalForSetupDefinitions.Single(x => x._setupDefinition.ResourceSetup.Id == fProposal.SetupId);

            proposalForSetupDefinition.Add(fProposal);
                return true;
        }

        public FOperation GetOperationBy(Guid operationKey)
        {
            return _proposalDictionary.SingleOrDefault(x => x.Key.Key == operationKey).Key;
        }

        internal bool RemoveAllProposalsFor(FOperation fOperation)
        {
            if (!_proposalDictionary.TryGetValue(fOperation, out var proposals))
                return false;

            proposals.ForEach(x => x.RemoveAll());
            return true;
        }

        public bool Remove(FOperation fOperation)
        {
            return _proposalDictionary.Remove(fOperation);
        }
    }
}
