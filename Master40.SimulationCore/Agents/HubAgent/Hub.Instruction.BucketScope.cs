using Akka.Actor;
using AkkaSim.Definitions;
using System;
using static FOperations;

namespace Master40.SimulationCore.Agents.HubAgent
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
            public class BucketScope
            {
                public class SetBucketFix : SimulationMessage
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

                public class DissolveBucket : SimulationMessage
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

                public class RequestFinalBucket : SimulationMessage
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

                public class EnqueueOperation : SimulationMessage
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

                public class EnqueueBucket : SimulationMessage
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

                public class ResponseRequeueBucket : SimulationMessage
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
