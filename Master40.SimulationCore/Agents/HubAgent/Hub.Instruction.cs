using Akka.Actor;
using AkkaSim.Definitions;
using static FProposals;
using static FResourceInformations;
using static IJobs;
using static FRequestToRequeues;
using static FBucketScopes;
using static FOperations;
using System;
using static FBuckets;
using static IJobResults;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public class Instruction
        {

            public class Default
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

            /// <summary>
            /// Implements the classes for BucketScope
            /// </summary>
            public class BucketScope
            {
                
                public class ResetBucket : SimulationMessage
                {
                    public static ResetBucket Create(Guid bucketKey, IActorRef target)
                    {
                        return new ResetBucket(message: bucketKey, target: target);
                    }
                    private ResetBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }

                public class SetBucketFix : SimulationMessage
                {
                    public static SetBucketFix Create(Guid key, IActorRef target)
                    {
                        return new SetBucketFix(message: key, target: target);
                    }
                    private SetBucketFix(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public Guid GetObjectFromMessage { get => (Guid)Message; }
                }
                public class EnqueueOperation : SimulationMessage
                {
                    public static EnqueueOperation Create(FOperation operation, IActorRef target)
                    {
                        return new EnqueueOperation(message: operation, target: target);
                    }
                    private EnqueueOperation(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FOperation GetObjectFromMessage { get => Message as FOperation; }
                }

                public class EnqueueBucket : SimulationMessage
                {
                    public static EnqueueBucket Create(FBucket bucket, IActorRef target)
                    {
                        return new EnqueueBucket(message: bucket, target: target);
                    }
                    private EnqueueBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FBucket GetObjectFromMessage { get => Message as FBucket; }
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
}