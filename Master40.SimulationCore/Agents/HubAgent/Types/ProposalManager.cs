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

        public bool Add(FOperation fOperation, List<SetupDefinition> setupDefinitions)
        {
            var defs = new List<ProposalForSetupDefinition>();
            setupDefinitions.ForEach(x => defs.Add(new ProposalForSetupDefinition(x)));
            return _proposalDictionary.TryAdd(fOperation, defs);
        }

        public FOperation GetOperationBy(Guid operationKey)
        {
            return _proposalDictionary.SingleOrDefault(x => x.Key.Key == operationKey).Key;
        }

    }
}
