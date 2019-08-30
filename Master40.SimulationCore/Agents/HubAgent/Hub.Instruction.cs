using Akka.Actor;
using AkkaSim.Definitions;
using static FAgentInformations;
using static FProposals;
using static FResourceInformations;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public partial class Hub
    {
        public class Instruction
        {
            public class AddResourceToHub : SimulationMessage
            {
                public static AddResourceToHub Create(FResourceInformation message, IActorRef target, bool logThis = false)
                {
                    return new AddResourceToHub(message: message, target: target, logThis: logThis);
                }
                private AddResourceToHub(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                {

                }
                public FResourceInformation GetObjectFromMessage { get => Message as FResourceInformation; }
            }

            public class EnqueueJob : SimulationMessage
            {
                public static EnqueueJob Create(IJob message, IActorRef target)
                {
                    return new EnqueueJob(message: message, target: target);
                }
                private EnqueueJob(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }

            public class ProposalFromResource : SimulationMessage
            {
                public static ProposalFromResource Create(FProposal message, IActorRef target)
                {
                    return new ProposalFromResource(message: message, target: target);
                }
                private ProposalFromResource(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public FProposal GetObjectFromMessage { get => Message as FProposal; }
            }

        }
    }
}