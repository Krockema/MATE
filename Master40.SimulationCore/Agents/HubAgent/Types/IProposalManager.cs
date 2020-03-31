using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    internal interface IProposalManager
    {
        

        bool AddProposal(FProposal proposal);

        bool RemoveProposal(FProposal proposal);

    }
}