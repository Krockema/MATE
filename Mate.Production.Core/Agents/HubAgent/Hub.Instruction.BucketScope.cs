using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using static FOperations;

namespace Mate.Production.Core.Agents.HubAgent
{
    partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public partial class Instruction
        {
            /// <summary>
            /// Implements the classes for BucketScope
            /// </summary>
            public record BucketScope
            {
                public record SetBucketFix : HiveMessage
                {
                    public static SetBucketFix Create(Guid key, IActorRef target)
                    {
                        return new SetBucketFix(message: key, target: target);
                    }

                    private SetBucketFix(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public Guid GetObjectFromMessage => (Guid) Message;
                }

                public record DissolveBucket : HiveMessage
                {
                    public static DissolveBucket Create(Guid key, IActorRef target)
                    {
                        return new DissolveBucket(message: key, target: target);
                    }

                    private DissolveBucket(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public Guid GetObjectFromMessage => (Guid)Message;
                }

                public record RequestFinalBucket : HiveMessage
                {
                    public static RequestFinalBucket Create(Guid key, IActorRef target)
                    {
                        return new RequestFinalBucket(message: key, target: target);
                    }

                    private RequestFinalBucket(object message, IActorRef target) : base(message: message,
                        target: target)
                    {

                    }

                    public Guid GetObjectFromMessage => (Guid) Message;
                }

                public record EnqueueOperation : HiveMessage
                {
                    public static EnqueueOperation Create(FOperations.FOperation operation, IActorRef target)
                    {
                        return new EnqueueOperation(message: operation, target: target);
                    }

                    private EnqueueOperation(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }

                    public FOperation GetObjectFromMessage => Message as FOperation;
                }

                public record EnqueueBucket : HiveMessage
                {
                    public static EnqueueBucket Create(Guid bucketKey, IActorRef target)
                    {
                        return new EnqueueBucket(message: bucketKey, target: target);
                    }

                    private EnqueueBucket(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }

                    public Guid GetObjectFromMessage => (Guid) Message;
                }

                public record ResponseRequeueBucket : HiveMessage
                {
                    public static ResponseRequeueBucket Create(FRequestToRequeues.FRequestToRequeue message,
                        IActorRef target)
                    {
                        return new ResponseRequeueBucket(message: message, target: target);
                    }

                    private ResponseRequeueBucket(object message, IActorRef target) : base(message: message,
                        target: target)
                    {

                    }

                    public FRequestToRequeues.FRequestToRequeue GetObjectFromMessage
                    {
                        get => Message as FRequestToRequeues.FRequestToRequeue;
                    }
                }
            }
        }
    }
}
