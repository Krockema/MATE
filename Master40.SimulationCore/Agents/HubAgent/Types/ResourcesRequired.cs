using System.Collections.Generic;
using Akka.Actor;
using Master40.DB.DataModel;
using static FProposals;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class ResourcesRequired
    {
        public M_Resource Resource { get; set; }
        public IActorRef ActorRef { get; set; }
        public FProposal Proposal { get; set; }
    }
}
