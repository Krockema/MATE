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
        private Dictionary<IJob, ProposalForSetupDefinitionSet> _proposalDictionary { get; set; } = new Dictionary<IJob, ProposalForSetupDefinitionSet>();

        public ProposalManager()
        {

        }

        public void ResetAssignedSetupDefinition(IJob job)
        {
            _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet);

            proposalForSetupDefinitionSet.ResetAssignedSetupDefinition();
        }

        public bool AllProposalForSetupDefinitionReceived(IJob job)
        {
            return _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet) 
                   && proposalForSetupDefinitionSet.getAllProposalForSetupDefinitions().TrueForAll(x => x.AllProposalsReceived());
        }

        public bool AllSetupDefintionsPostponed(IJob job)
        {
            return _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet)
                   && proposalForSetupDefinitionSet.getAllProposalForSetupDefinitions().TrueForAll(x => x.AnyPostponed());
        }

        public long PostponedUntil(IJob job)
        {
            _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet);

            return proposalForSetupDefinitionSet.getAllProposalForSetupDefinitions().Min(x => x.PostponedUntil());
        }

        public bool Add(IJob job, List<FSetupDefinition> fSetupDefinitions)
        {
            var defs = new ProposalForSetupDefinitionSet();
            fSetupDefinitions.ForEach(x => defs.Add(new ProposalForSetupDefinition(x)));
            return _proposalDictionary.TryAdd(job, defs);
        }

        public bool AddProposal(FProposal fProposal)
        {
            var job = GetJobBy(fProposal.JobKey).Key;

            if (!_proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet))
                return false;

            var proposalForSetupDefinition = proposalForSetupDefinitionSet.getAllProposalForSetupDefinitions().Single(x => x.GetFSetupDefinition.SetupKey == fProposal.SetupId);

            proposalForSetupDefinition.Add(fProposal);
                return true;
        }

        public KeyValuePair<IJob, ProposalForSetupDefinitionSet> GetJobBy(Guid jobKey)
        {
            return _proposalDictionary.SingleOrDefault(x => x.Key.Key == jobKey);
        }

        internal bool RemoveAllProposalsFor(IJob job)
        {
            if (!_proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet))
                return false;

            job;
            proposalForSetupDefinitionSet.ResetAssignedSetupDefinition();
            proposalForSetupDefinitionSet.getAllProposalForSetupDefinitions().ForEach(x => x.RemoveAll());
            return true;
        }

        public bool Remove(IJob job)
        {
            return _proposalDictionary.Remove(job);
        }

        internal FSetupDefinition GetAssignedSetupDefinition(IJob job)
        {
            _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet);
            return proposalForSetupDefinitionSet?.AssignedSetupDefinition;
        }

        internal ProposalForSetupDefinition GetValidProposalForSetupDefinitionFor(IJob job)
        {
            _proposalDictionary.TryGetValue(job, out var proposalForSetupDefinitionSet);
            return proposalForSetupDefinitionSet.GetValidProposal();
        }

        internal void Update(IJob job)
        {
            var value = _proposalDictionary.Single(x => x.Key.Key == job.Key);
            _proposalDictionary.Remove(value.Key);
            _proposalDictionary.Add(job, value.Value);
        }
    }
}
