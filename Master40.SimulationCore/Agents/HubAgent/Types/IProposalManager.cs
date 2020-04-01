using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    internal interface IProposalManager
    {
        void Add(FProposal proposal);
        void Remove(long setupId);

    }
}