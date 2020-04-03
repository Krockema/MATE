using Akka.Actor;
using AkkaSim.Definitions;
using System;
using Master40.SimulationCore.Agents.HubAgent.Types;
using static FAgentInformations;
using static FBuckets;
using static FBucketScopes;
using static FRequestToRequeues;
using static FUpdateBucketScopes;
using static IJobResults;
using static IJobs;
using static FRequestProposalForSetups;
using static FAcknowledgeProposals;
using static FJobConfirmations;

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
                    public static RequestProposal Create(FRequestProposalForSetup message, IActorRef target)
                    {
                        return new RequestProposal(message: message, target: target);
                    }
                    private RequestProposal(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FRequestProposalForSetup GetObjectFromMessage { get => Message as FRequestProposalForSetup; }
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



                public class RequestToRequeue : SimulationMessage
                {
                    public static RequestToRequeue Create(FRequestToRequeue message, IActorRef target)
                    {
                        return new RequestToRequeue(message: message, target: target);
                    }
                    private RequestToRequeue(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FRequestToRequeue GetObjectFromMessage { get => Message as FRequestToRequeue; }
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
                    public static AcknowledgeJob Create(JobAcknowledgement message, IActorRef target)
                    {
                        return new AcknowledgeJob(message: message, target: target);
                    }
                    private AcknowledgeJob(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public JobAcknowledgement GetObjectFromMessage { get => Message as JobAcknowledgement; }
                }
                public class EnqueueBucket : SimulationMessage
                {
                    public static EnqueueBucket Create(FBucket message, IActorRef target)
                    {
                        return new EnqueueBucket(message: message, target: target);
                    }
                    private EnqueueBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FBucket GetObjectFromMessage { get => Message as FBucket; }
                }
                public class UpdateBucket : SimulationMessage
                {
                    public static UpdateBucket Create(FBucket message, IActorRef target)
                    {
                        return new UpdateBucket(message: message, target: target);
                    }
                    private UpdateBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FBucket GetObjectFromMessage { get => Message as FBucket; }
                }

                public class RequestProposalForBucketScope : SimulationMessage
                {
                    public static RequestProposalForBucketScope Create(FBucket message, IActorRef target)
                    {
                        return new RequestProposalForBucketScope(message: message, target: target);
                    }
                    private RequestProposalForBucketScope(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FBucket GetObjectFromMessage { get => Message as FBucket; }
                }

                public class AcknowledgeBucketScope : SimulationMessage
                {
                    public static AcknowledgeBucketScope Create(Guid message, IActorRef target)
                    {
                        return new AcknowledgeBucketScope(message: message, target: target);
                    }
                    private AcknowledgeBucketScope(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }
                public class ScopeHasSatisfiedJob : SimulationMessage
                {
                    public static ScopeHasSatisfiedJob Create(Guid message, IActorRef target)
                    {
                        return new ScopeHasSatisfiedJob(message: message, target: target);
                    }
                    private ScopeHasSatisfiedJob(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
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

                public class EnqueueProcessingQueue : SimulationMessage
                {
                    public static EnqueueProcessingQueue Create(FBucket message, IActorRef target)
                    {
                        return new EnqueueProcessingQueue(message: message, target: target);
                    }
                    private EnqueueProcessingQueue(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FBucket GetObjectFromMessage { get => Message as FBucket; }
                }

                public class BucketReady : SimulationMessage
                {
                    public static BucketReady Create(Guid message, IActorRef target)
                    {
                        return new BucketReady(message: message, target: target);
                    }
                    private BucketReady(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

            }
        }


    }
}