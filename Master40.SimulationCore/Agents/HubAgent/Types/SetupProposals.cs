using System.Collections.Generic;
using Master40.DB.DataModel;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class SetupProposals
    {
        List<FProposal> proposals = new List<FProposal>();
        private SetupDefinition setupDefinition; // allways setup that have been Requested. (No parent.)
    }
}