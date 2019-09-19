using Akka.Actor;
using AkkaSim.Definitions;
using static FProposals;
using static FResourceInformations;
using static IJobs;
using static FRequestToRequeues;

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

            public class EnqueueBucket : SimulationMessage
            {
                public static EnqueueBucket Create(IJob message, IActorRef target)
                {
                    return new EnqueueBucket(message: message, target: target);
                }
                private EnqueueBucket(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }

            public class SetJobFix : SimulationMessage
            {
                public static SetJobFix Create(IJob message, IActorRef target)
                {
                    return new SetJobFix(message: message, target: target);
                }
                private SetJobFix(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }

            public class RequeueBucket : SimulationMessage
            {
                public static RequeueBucket Create(FRequestToRequeue message, IActorRef target)
                {
                    return new RequeueBucket(message: message, target: target);
                }
                private RequeueBucket(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public FRequestToRequeue GetObjectFromMessage { get => Message as FRequestToRequeue; }
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

            public class ResponseRequeueBucket : SimulationMessage
            {
                public static ResponseRequeueBucket Create(FRequestToRequeue message, IActorRef target)
                {
                    return new ResponseRequeueBucket(message: message, target: target);
                }
                private ResponseRequeueBucket(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public FRequestToRequeue GetObjectFromMessage { get => Message as FRequestToRequeue; }
            }
        }
    }
}