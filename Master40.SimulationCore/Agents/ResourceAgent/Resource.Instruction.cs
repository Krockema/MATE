using Akka.Actor;
using AkkaSim.Definitions;
using System;
using static FAgentInformations;
using static FBuckets;
using static FJobConfirmations;
using static FRequestProposalForCapabilityProviders;
using static FRequestToRequeues;
using static IJobResults;

namespace Master40.SimulationCore.Agents.ResourceAgent
{
    public partial class Resource
    {
        public class Instruction
        {
            public class Default
            {
                public class SetHubAgent : SimulationMessage
                {
                    public static SetHubAgent Create(FAgentInformation message, IActorRef target)
                    {
                        return new SetHubAgent(message: message, target: target);
                    }
                    private SetHubAgent(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
                }

                public class RequestProposal : SimulationMessage
                {
                    public static RequestProposal Create(FRequestProposalForCapabilityProvider message, IActorRef target)
                    {
                        return new RequestProposal(message: message, target: target);
                    }
                    private RequestProposal(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FRequestProposalForCapabilityProvider GetObjectFromMessage { get => Message as FRequestProposalForCapabilityProvider; }
                }

                public class AcknowledgeProposal : SimulationMessage
                {
                    public static AcknowledgeProposal Create(FJobConfirmation message, IActorRef target)
                    {
                        return new AcknowledgeProposal(message: message, target: target);
                    }
                    private AcknowledgeProposal(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FJobConfirmation GetObjectFromMessage { get => Message as FJobConfirmation; }
                }

                public class DoWork : SimulationMessage
                {
                    public static DoWork Create(object message, IActorRef target)
                    {
                        return new DoWork(message: message, target: target);
                    }
                    private DoWork(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                }
            }

            public class BucketScope
            {
                public class RequeueBucket : SimulationMessage
                {
                    public static RequeueBucket Create(Guid message, IActorRef target)
                    {
                        return new RequeueBucket(message: message, target: target);
                    }
                    private RequeueBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

                public class AcknowledgeJob : SimulationMessage
                {
                    public static AcknowledgeJob Create(FJobConfirmation message, IActorRef target)
                    {
                        return new AcknowledgeJob(message: message, target: target);
                    }
                    private AcknowledgeJob(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FJobConfirmation GetObjectFromMessage { get => Message as FJobConfirmation; }
                }

                public class FinishBucket : SimulationMessage
                {
                    public static FinishBucket Create(IJobResult message, IActorRef target)
                    {
                        return new FinishBucket(message: message, target: target);
                    }
                    private FinishBucket(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public IJobResult GetObjectFromMessage { get => Message as IJobResult; }
                }

                public class AskToRequeue : SimulationMessage
                {
                    public static AskToRequeue Create(Guid jobKey, IActorRef target)
                    {
                        return new AskToRequeue(message: jobKey, target: target);
                    }
                    private AskToRequeue(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

            }
        }


    }
}